/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity;
using NUnit.Framework;

namespace ArenaUnity.Tests
{
    /// <summary>
    /// ArenaMqttClient.MqttTopicMatch is the MQTT topic-filter matcher used by
    /// HasPerms (ArenaMqttClient.cs:239-245) to decide whether the publish
    /// permissions in a signed JWT cover the topic we are about to publish to. It is
    /// therefore a security boundary, not just a string utility, and both its
    /// positive and negative answers are worth pinning.
    ///
    /// It lives here rather than in the plain .NET suite only because
    /// ArenaMqttClient derives from a MonoBehaviour; the method itself is static and
    /// needs no instance.
    /// </summary>
    [TestFixture]
    public class ArenaTopicMatchTests
    {
        // -------------------------------------------------- MQTT wildcard semantics

        [TestCase("a/b/c", "a/b/c", true, TestName = "Match_ExactTopic")]
        [TestCase("a/b/c", "a/b/d", false, TestName = "Match_ExactTopic_LastLevelDiffers")]
        [TestCase("a/b/c", "x/b/c", false, TestName = "Match_ExactTopic_FirstLevelDiffers")]
        [TestCase("a/B/c", "a/b/c", false, TestName = "Match_IsCaseSensitive")]

        // Single-level wildcard: matches exactly one level, never more or fewer.
        [TestCase("a/+/c", "a/x/c", true, TestName = "Match_Plus_MatchesOneLevel")]
        [TestCase("a/+/c", "a/x/y", false, TestName = "Match_Plus_StillRequiresTheRest")]
        [TestCase("a/+", "a/x", true, TestName = "Match_Plus_AtEnd")]
        [TestCase("a/+", "a/x/y", false, TestName = "Match_Plus_DoesNotSpanLevels")]
        [TestCase("+/b", "a/b", true, TestName = "Match_Plus_AtStart")]
        [TestCase("+", "a", true, TestName = "Match_Plus_Alone")]
        [TestCase("+", "a/b", false, TestName = "Match_Plus_AloneDoesNotSpanLevels")]
        [TestCase("+/+/+", "a/b/c", true, TestName = "Match_AllPlus")]
        [TestCase("+/+/+", "a/b", false, TestName = "Match_AllPlus_TooFewLevels")]

        // Multi-level wildcard: matches the remainder, including the empty remainder.
        [TestCase("a/#", "a/b/c", true, TestName = "Match_Hash_MatchesManyLevels")]
        [TestCase("a/#", "a/b", true, TestName = "Match_Hash_MatchesOneLevel")]
        [TestCase("a/#", "a", true, TestName = "Match_Hash_MatchesParentLevelItself")]
        [TestCase("a/#", "b/c", false, TestName = "Match_Hash_StillRequiresThePrefix")]
        [TestCase("#", "a/b/c", true, TestName = "Match_Hash_Alone_MatchesEverything")]
        [TestCase("a/b/#", "a/b", true, TestName = "Match_Hash_AfterExactPrefix")]
        [TestCase("a/+/#", "a/b/c/d", true, TestName = "Match_PlusThenHash")]

        // Length mismatches with no wildcard left to absorb them.
        [TestCase("a/b", "a/b/c", false, TestName = "Match_FilterShorterThanTopic")]
        [TestCase("a/b/c", "a/b", false, TestName = "Match_FilterLongerThanTopic")]
        [TestCase("a/b/c", "a/b/c/d", false, TestName = "Match_TopicHasExtraTrailingLevel")]
        [TestCase("a/b/", "a/b", false, TestName = "Match_TrailingSlashIsItsOwnEmptyLevel")]
        public void MqttTopicMatch_WildcardMatrix(string filter, string topic, bool expected)
        {
            Assert.That(ArenaMqttClient.MqttTopicMatch(filter, topic), Is.EqualTo(expected));
        }

        // ------------------------------------ JWT publish-permission scenarios

        /// <summary>
        /// A fixture whose toUid equals its idTag, modelling "a message addressed to
        /// me": that is the case in which a private subscription filter is expected to
        /// match a private publish topic.
        /// </summary>
        private static ArenaTopics SelfAddressed()
        {
            return new ArenaTopics(
                realm: "realm", name_space: "ns1", scenename: "scene1",
                userclient: "uc", idtag: "id", uuId: "uu", userobj: "uo",
                objectid: "obj", touid: "id", devicename: "dev");
        }

        /// <summary>
        /// The broad grant a scene editor's token carries must cover every topic the
        /// client publishes into that scene. If this ever fails, publishes start being
        /// rejected by HasPerms.
        /// </summary>
        [Test]
        public void SceneWideGrant_CoversEveryScenePublishTopic()
        {
            var t = SelfAddressed();
            const string grant = "realm/s/ns1/scene1/#";

            foreach (string topic in new[]
            {
                t.PUB_SCENE_PRESENCE, t.PUB_SCENE_PRESENCE_PRIVATE,
                t.PUB_SCENE_CHAT, t.PUB_SCENE_CHAT_PRIVATE,
                t.PUB_SCENE_USER, t.PUB_SCENE_USER_PRIVATE,
                t.PUB_SCENE_OBJECTS, t.PUB_SCENE_OBJECTS_PRIVATE,
                t.PUB_SCENE_RENDER, t.PUB_SCENE_RENDER_PRIVATE, t.PUB_SCENE_RENDER_PRI_SERV,
                t.PUB_SCENE_ENV, t.PUB_SCENE_ENV_PRIVATE,
                t.PUB_SCENE_PROGRAM, t.PUB_SCENE_PROGRAM_PRIVATE,
                t.PUB_SCENE_DEBUG,
            })
            {
                Assert.That(ArenaMqttClient.MqttTopicMatch(grant, topic), Is.True, topic);
            }
        }

        /// <summary>
        /// The security-relevant direction: a grant scoped to one scene must not cover
        /// another scene, and a grant scoped to one userclient must not cover topics
        /// published under a different userclient.
        /// </summary>
        [Test]
        public void NarrowGrant_DoesNotCoverAnotherScopesTopics()
        {
            var t = SelfAddressed();

            Assert.That(ArenaMqttClient.MqttTopicMatch("realm/s/ns1/otherscene/#", t.PUB_SCENE_OBJECTS),
                Is.False, "a grant for another scene must not cover this scene");
            Assert.That(ArenaMqttClient.MqttTopicMatch("realm/s/othernamespace/scene1/#", t.PUB_SCENE_OBJECTS),
                Is.False, "a grant for another namespace must not cover this namespace");
            Assert.That(ArenaMqttClient.MqttTopicMatch("realm/s/ns1/scene1/+/otheruc/#", t.PUB_SCENE_OBJECTS),
                Is.False, "a grant for another userclient must not cover our publishes");
            Assert.That(ArenaMqttClient.MqttTopicMatch("otherrealm/s/ns1/scene1/#", t.PUB_SCENE_OBJECTS),
                Is.False, "a grant in another realm must not cover this realm");
        }

        [Test]
        public void PerUserclientGrant_CoversOurOwnPublishTopics()
        {
            var t = SelfAddressed();
            const string grant = "realm/s/ns1/scene1/+/uc/#";

            Assert.That(ArenaMqttClient.MqttTopicMatch(grant, t.PUB_SCENE_OBJECTS), Is.True);
            Assert.That(ArenaMqttClient.MqttTopicMatch(grant, t.PUB_SCENE_CHAT), Is.True);
            Assert.That(ArenaMqttClient.MqttTopicMatch(grant, t.PUB_SCENE_PRESENCE_PRIVATE), Is.True);
        }

        // ------------------------- cross-class: SUB_* filters vs PUB_* topics

        /// <summary>
        /// The public scene subscription is what every client uses to see scene traffic,
        /// so it must match all seven non-private scene publish topics. This is the
        /// cross-class invariant that ties ArenaTopics to the matcher: change either
        /// side alone and clients stop receiving objects.
        /// </summary>
        [Test]
        public void SubScenePublic_MatchesEveryNonPrivateScenePublishTopic()
        {
            var t = SelfAddressed();
            string filter = t.SUB_SCENE_PUBLIC;

            foreach (string topic in new[]
            {
                t.PUB_SCENE_PRESENCE, t.PUB_SCENE_CHAT, t.PUB_SCENE_USER,
                t.PUB_SCENE_OBJECTS, t.PUB_SCENE_RENDER, t.PUB_SCENE_ENV,
                t.PUB_SCENE_PROGRAM,
            })
            {
                Assert.That(ArenaMqttClient.MqttTopicMatch(filter, topic), Is.True, topic);
            }
        }

        /// <summary>
        /// The privacy invariant, and the reason SUB_SCENE_PUBLIC has exactly three
        /// trailing "+" levels rather than a "#": an eight-segment private topic must
        /// NOT be delivered to the public subscription.
        /// </summary>
        [Test]
        public void SubScenePublic_DoesNotMatchPrivatePublishTopics()
        {
            var t = SelfAddressed();
            string filter = t.SUB_SCENE_PUBLIC;

            foreach (string topic in new[]
            {
                t.PUB_SCENE_PRESENCE_PRIVATE, t.PUB_SCENE_CHAT_PRIVATE,
                t.PUB_SCENE_USER_PRIVATE, t.PUB_SCENE_OBJECTS_PRIVATE,
                t.PUB_SCENE_PROGRAM_PRIVATE, t.PUB_SCENE_RENDER_PRIVATE,
                t.PUB_SCENE_RENDER_PRI_SERV, t.PUB_SCENE_ENV_PRIVATE,
                t.PUB_SCENE_DEBUG,
            })
            {
                Assert.That(ArenaMqttClient.MqttTopicMatch(filter, topic), Is.False, topic);
            }
        }

        /// <summary>
        /// The private subscription ends in "/{idTag}/#", so it matches exactly those
        /// private topics whose recipient slot is our own idTag.
        /// </summary>
        [Test]
        public void SubScenePrivate_MatchesPrivateTopicsAddressedToUs()
        {
            var t = SelfAddressed();
            string filter = t.SUB_SCENE_PRIVATE;

            foreach (string topic in new[]
            {
                t.PUB_SCENE_PRESENCE_PRIVATE, t.PUB_SCENE_CHAT_PRIVATE,
                t.PUB_SCENE_USER_PRIVATE, t.PUB_SCENE_OBJECTS_PRIVATE,
                t.PUB_SCENE_PROGRAM_PRIVATE, t.PUB_SCENE_RENDER_PRI_SERV,
            })
            {
                Assert.That(ArenaMqttClient.MqttTopicMatch(filter, topic), Is.True, topic);
            }
        }

        /// <summary>
        /// Conversely, a private topic addressed to somebody else must not reach us.
        /// </summary>
        [Test]
        public void SubScenePrivate_DoesNotMatchTopicsAddressedToSomeoneElse()
        {
            var mine = new ArenaTopics(realm: "realm", name_space: "ns1", scenename: "scene1",
                userclient: "uc", idtag: "id", objectid: "obj", touid: "id");
            var theirs = new ArenaTopics(realm: "realm", name_space: "ns1", scenename: "scene1",
                userclient: "uc", idtag: "id", objectid: "obj", touid: "someoneelse");

            Assert.That(ArenaMqttClient.MqttTopicMatch(mine.SUB_SCENE_PRIVATE, theirs.PUB_SCENE_OBJECTS_PRIVATE),
                Is.False);
        }

        /// <summary>
        /// RENDER_PRIVATE, ENV_PRIVATE and DEBUG publish to a literal "-" recipient
        /// precisely so that no other client's private subscription picks them up. That
        /// is what the "// To avoid unpriv sub" comments in ArenaTopics.cs mean, and it
        /// only holds because "-" never equals anybody's idTag.
        /// </summary>
        [Test]
        public void DashAddressedTopics_ReachNobodysPrivateSubscription()
        {
            var t = SelfAddressed();
            string filter = t.SUB_SCENE_PRIVATE;

            Assert.That(ArenaMqttClient.MqttTopicMatch(filter, t.PUB_SCENE_RENDER_PRIVATE), Is.False);
            Assert.That(ArenaMqttClient.MqttTopicMatch(filter, t.PUB_SCENE_ENV_PRIVATE), Is.False);
            Assert.That(ArenaMqttClient.MqttTopicMatch(filter, t.PUB_SCENE_DEBUG), Is.False);
        }

        /// <summary>
        /// The render-specific private subscription is narrower than SUB_SCENE_PRIVATE:
        /// it pins the message type to "r", so it must match the server's private render
        /// topic and not, say, a private object update.
        /// </summary>
        [Test]
        public void SubSceneRenderPrivate_MatchesOnlyRenderTraffic()
        {
            var t = SelfAddressed();
            string filter = t.SUB_SCENE_RENDER_PRIVATE;

            Assert.That(ArenaMqttClient.MqttTopicMatch(filter, t.PUB_SCENE_RENDER_PRI_SERV), Is.True);
            Assert.That(ArenaMqttClient.MqttTopicMatch(filter, t.PUB_SCENE_OBJECTS_PRIVATE), Is.False);
            Assert.That(ArenaMqttClient.MqttTopicMatch(filter, t.PUB_SCENE_CHAT_PRIVATE), Is.False);
        }

        [Test]
        public void DeviceAndProcessFilters_MatchTheirOwnPublishTopics()
        {
            var t = SelfAddressed();

            Assert.That(ArenaMqttClient.MqttTopicMatch(t.SUB_DEVICE, t.PUB_DEVICE), Is.True);
            Assert.That(ArenaMqttClient.MqttTopicMatch(t.SUB_PROC_REG, t.PUB_PROC_REG), Is.True);
            Assert.That(ArenaMqttClient.MqttTopicMatch(t.SUB_PROC_DBG, t.PUB_PROC_DBG), Is.True);
        }

        /// <summary>
        /// Two documented non-matches, recorded so that neither reads as an accident:
        ///
        /// SUB_PROC_CTL is "{realm}/proc/control/{uuid}/#" while PUB_PROC_CTL is the
        /// bare "{realm}/proc/control". The client publishes control registrations to
        /// the bare topic and listens for per-module control messages underneath its own
        /// uuid, so these two are deliberately different topics rather than a
        /// subscribe/publish pair.
        ///
        /// SUB_NETWORK is "$NETWORK" while PUB_NETWORK_LATENCY is "$NETWORK/latency",
        /// and under MQTT semantics the former does not match the latter. Neither is
        /// referenced anywhere else in the package today; they are placeholders for the
        /// latency-measurement channel the web client implements. If latency
        /// measurement is ever wired up here, SUB_NETWORK will need a "/#" suffix - this
        /// test is where that shows up.
        /// </summary>
        [Test]
        public void DocumentedNonPairs_DoNotMatch()
        {
            var t = SelfAddressed();

            Assert.That(ArenaMqttClient.MqttTopicMatch(t.SUB_PROC_CTL, t.PUB_PROC_CTL), Is.False);
            Assert.That(ArenaMqttClient.MqttTopicMatch(t.SUB_NETWORK, t.PUB_NETWORK_LATENCY), Is.False);
        }
    }
}
