/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ArenaUnity;
using NUnit.Framework;

namespace ArenaUnity.PureTests
{
    /// <summary>
    /// Exact-string contract tests for every ArenaTopics getter.
    ///
    /// CONTRIBUTING.md rule #1 forbids hardcoding MQTT topic strings anywhere but
    /// ArenaTopics, and the topic layout is a cross-language wire protocol shared
    /// with arena-py and arena-web-core. These strings are therefore a public
    /// contract: if one of them has to change, the corresponding expectation below
    /// must be changed deliberately, in the same commit.
    /// </summary>
    [TestFixture]
    public class ArenaTopicsTests
    {
        // A distinctive fixture: every constructor argument is a different, short,
        // unambiguous token so a mis-slotted argument shows up as a diff, not as a
        // coincidental match.
        private const string Realm = "realm";
        private const string NameSpace = "ns1";
        private const string SceneName = "scene1";
        private const string UserClient = "uc";
        private const string IdTag = "id";
        private const string Uuid = "uu";
        private const string UserObj = "uo";
        private const string ObjectId = "obj";
        private const string ToUid = "to";
        private const string DeviceName = "dev";

        private static ArenaTopics Fixture()
        {
            return new ArenaTopics(
                realm: Realm,
                name_space: NameSpace,
                scenename: SceneName,
                userclient: UserClient,
                idtag: IdTag,
                uuId: Uuid,
                userobj: UserObj,
                objectid: ObjectId,
                touid: ToUid,
                devicename: DeviceName);
        }

        /// <summary>
        /// The complete expected topic table. Key = getter name, value = exact string.
        /// </summary>
        private static readonly Dictionary<string, string> ExpectedTopics = new Dictionary<string, string>
        {
            // SUBSCRIBE
            { "SUB_NETWORK",               "$NETWORK" },
            { "SUB_DEVICE",                "realm/d/ns1/dev/#" },
            { "SUB_PROC_REG",              "realm/proc/reg" },
            { "SUB_PROC_CTL",              "realm/proc/control/uu/#" },
            { "SUB_PROC_DBG",              "realm/proc/debug/uu" },
            { "SUB_SCENE_PUBLIC",          "realm/s/ns1/scene1/+/+/+" },
            { "SUB_SCENE_PRIVATE",         "realm/s/ns1/scene1/+/+/+/id/#" },
            { "SUB_SCENE_RENDER_PRIVATE",  "realm/s/ns1/scene1/r/+/+/id/#" },

            // PUBLISH
            { "PUB_NETWORK_LATENCY",       "$NETWORK/latency" },
            { "PUB_DEVICE",                "realm/d/ns1/dev/id" },
            { "PUB_PROC_REG",              "realm/proc/reg" },
            { "PUB_PROC_CTL",              "realm/proc/control" },
            { "PUB_PROC_DBG",              "realm/proc/debug/uu" },
            { "PUB_SCENE_PRESENCE",        "realm/s/ns1/scene1/x/uc/id" },
            { "PUB_SCENE_PRESENCE_PRIVATE","realm/s/ns1/scene1/x/uc/id/to" },
            { "PUB_SCENE_CHAT",            "realm/s/ns1/scene1/c/uc/id" },
            { "PUB_SCENE_CHAT_PRIVATE",    "realm/s/ns1/scene1/c/uc/id/to" },
            { "PUB_SCENE_USER",            "realm/s/ns1/scene1/u/uc/uo" },
            { "PUB_SCENE_USER_PRIVATE",    "realm/s/ns1/scene1/u/uc/uo/to" },
            { "PUB_SCENE_OBJECTS",         "realm/s/ns1/scene1/o/uc/obj" },
            { "PUB_SCENE_OBJECTS_PRIVATE", "realm/s/ns1/scene1/o/uc/obj/to" },
            { "PUB_SCENE_RENDER",          "realm/s/ns1/scene1/r/uc/id" },
            { "PUB_SCENE_RENDER_PRIVATE",  "realm/s/ns1/scene1/r/uc/id/-" },
            { "PUB_SCENE_RENDER_PRI_SERV", "realm/s/ns1/scene1/r/uc/-/to" },
            { "PUB_SCENE_ENV",             "realm/s/ns1/scene1/e/uc/id" },
            { "PUB_SCENE_ENV_PRIVATE",     "realm/s/ns1/scene1/e/uc/id/-" },
            { "PUB_SCENE_PROGRAM",         "realm/s/ns1/scene1/p/uc/id" },
            { "PUB_SCENE_PROGRAM_PRIVATE", "realm/s/ns1/scene1/p/uc/id/to" },
            { "PUB_SCENE_DEBUG",           "realm/s/ns1/scene1/d/uc/id/-" },
        };

        private static IEnumerable<TestCaseData> TopicCases()
        {
            foreach (var kv in ExpectedTopics)
                yield return new TestCaseData(kv.Key, kv.Value).SetName($"Topic_{kv.Key}");
        }

        private static string GetTopic(ArenaTopics topics, string getterName)
        {
            PropertyInfo prop = typeof(ArenaTopics).GetProperty(
                getterName, BindingFlags.Public | BindingFlags.Instance);
            Assert.That(prop, Is.Not.Null, $"ArenaTopics has no public property '{getterName}'");
            return (string)prop.GetValue(topics);
        }

        [TestCaseSource(nameof(TopicCases))]
        public void Topic_MatchesExactString(string getterName, string expected)
        {
            Assert.That(GetTopic(Fixture(), getterName), Is.EqualTo(expected));
        }

        /// <summary>
        /// Drift guard: the expectation table above must cover every public string
        /// getter on ArenaTopics, no more and no less. Adding a topic without adding
        /// its expected string fails here.
        /// </summary>
        [Test]
        public void EveryPublicTopicGetter_IsCovered()
        {
            var declared = typeof(ArenaTopics)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType == typeof(string))
                .Select(p => p.Name)
                .Where(n => n.StartsWith("SUB_") || n.StartsWith("PUB_"))
                .OrderBy(n => n)
                .ToArray();

            Assert.That(declared, Is.EquivalentTo(ExpectedTopics.Keys),
                "ArenaTopics topic getters and the ExpectedTopics table have diverged.");
            Assert.That(declared.Length, Is.EqualTo(29),
                "Topic getter count changed; this is a wire-protocol change.");
        }

        /// <summary>
        /// The ten constructor arguments must land in the ten matching properties.
        /// Guards against a re-ordered parameter list silently swapping, say,
        /// idTag and userClient.
        /// </summary>
        [Test]
        public void Constructor_AssignsEveryArgumentToItsOwnProperty()
        {
            var t = Fixture();
            Assert.Multiple(() =>
            {
                Assert.That(t.REALM, Is.EqualTo(Realm));
                Assert.That(t.nameSpace, Is.EqualTo(NameSpace));
                Assert.That(t.sceneName, Is.EqualTo(SceneName));
                Assert.That(t.userClient, Is.EqualTo(UserClient));
                Assert.That(t.idTag, Is.EqualTo(IdTag));
                Assert.That(t.uuid, Is.EqualTo(Uuid));
                Assert.That(t.userObj, Is.EqualTo(UserObj));
                Assert.That(t.objectId, Is.EqualTo(ObjectId));
                Assert.That(t.toUid, Is.EqualTo(ToUid));
                Assert.That(t.deviceName, Is.EqualTo(DeviceName));
            });
        }

        // ---------------------------------------------------------------------
        // Positional invariants: ArenaTopicTokens indices are consumed directly by
        // the inbound router (ArenaClientScene.Messaging.cs reads
        // topicSplit[(int)ArenaTopicTokens.SCENE_MSGTYPE] and friends), so the slot
        // that each value occupies is as much a contract as the string itself.
        // ---------------------------------------------------------------------

        private static IEnumerable<TestCaseData> PubSceneCases()
        {
            yield return new TestCaseData("PUB_SCENE_PRESENCE", ArenaTopicSceneMsgTypes.PRESENCE);
            yield return new TestCaseData("PUB_SCENE_PRESENCE_PRIVATE", ArenaTopicSceneMsgTypes.PRESENCE);
            yield return new TestCaseData("PUB_SCENE_CHAT", ArenaTopicSceneMsgTypes.CHAT);
            yield return new TestCaseData("PUB_SCENE_CHAT_PRIVATE", ArenaTopicSceneMsgTypes.CHAT);
            yield return new TestCaseData("PUB_SCENE_USER", ArenaTopicSceneMsgTypes.USER);
            yield return new TestCaseData("PUB_SCENE_USER_PRIVATE", ArenaTopicSceneMsgTypes.USER);
            yield return new TestCaseData("PUB_SCENE_OBJECTS", ArenaTopicSceneMsgTypes.OBJECTS);
            yield return new TestCaseData("PUB_SCENE_OBJECTS_PRIVATE", ArenaTopicSceneMsgTypes.OBJECTS);
            yield return new TestCaseData("PUB_SCENE_RENDER", ArenaTopicSceneMsgTypes.RENDER);
            yield return new TestCaseData("PUB_SCENE_RENDER_PRIVATE", ArenaTopicSceneMsgTypes.RENDER);
            yield return new TestCaseData("PUB_SCENE_RENDER_PRI_SERV", ArenaTopicSceneMsgTypes.RENDER);
            yield return new TestCaseData("PUB_SCENE_ENV", ArenaTopicSceneMsgTypes.ENV);
            yield return new TestCaseData("PUB_SCENE_ENV_PRIVATE", ArenaTopicSceneMsgTypes.ENV);
            yield return new TestCaseData("PUB_SCENE_PROGRAM", ArenaTopicSceneMsgTypes.PROGRAM);
            yield return new TestCaseData("PUB_SCENE_PROGRAM_PRIVATE", ArenaTopicSceneMsgTypes.PROGRAM);
            yield return new TestCaseData("PUB_SCENE_DEBUG", ArenaTopicSceneMsgTypes.DEBUG);
        }

        [TestCaseSource(nameof(PubSceneCases))]
        public void PubSceneTopic_PutsEveryTokenInItsDeclaredSlot(string getterName, string expectedMsgType)
        {
            string[] parts = GetTopic(Fixture(), getterName).Split('/');

            Assert.That(parts.Length, Is.GreaterThan((int)ArenaTopicTokens.UUID),
                "a scene topic must have at least REALM..UUID slots");
            Assert.Multiple(() =>
            {
                Assert.That(parts[(int)ArenaTopicTokens.REALM], Is.EqualTo(Realm));
                Assert.That(parts[(int)ArenaTopicTokens.TYPE], Is.EqualTo("s"));
                Assert.That(parts[(int)ArenaTopicTokens.NAMESPACE], Is.EqualTo(NameSpace));
                Assert.That(parts[(int)ArenaTopicTokens.SCENENAME], Is.EqualTo(SceneName));
                Assert.That(parts[(int)ArenaTopicTokens.SCENE_MSGTYPE], Is.EqualTo(expectedMsgType));
                Assert.That(parts[(int)ArenaTopicTokens.USER_CLIENT], Is.EqualTo(UserClient));
            });
        }

        [Test]
        public void PubScenePrivateTopics_PutRecipientInToUidSlot()
        {
            var t = Fixture();
            foreach (var getterName in new[]
            {
                "PUB_SCENE_PRESENCE_PRIVATE", "PUB_SCENE_CHAT_PRIVATE",
                "PUB_SCENE_USER_PRIVATE", "PUB_SCENE_OBJECTS_PRIVATE",
                "PUB_SCENE_PROGRAM_PRIVATE", "PUB_SCENE_RENDER_PRI_SERV",
            })
            {
                string[] parts = GetTopic(t, getterName).Split('/');
                Assert.That(parts.Length, Is.EqualTo((int)ArenaTopicTokens.TO_UID + 1),
                    $"{getterName} must have exactly a TO_UID slot and nothing after it");
                Assert.That(parts[(int)ArenaTopicTokens.TO_UID], Is.EqualTo(ToUid), getterName);
            }
        }

        /// <summary>
        /// RENDER_PRIVATE, ENV_PRIVATE and DEBUG deliberately publish to a literal
        /// "-" recipient rather than a real uid, to keep unprivileged subscribers
        /// from matching the public wildcard filter. That "-" is load-bearing.
        /// </summary>
        [Test]
        public void SelfPrivateTopics_UseDashRecipientToAvoidUnprivilegedSubscribers()
        {
            var t = Fixture();
            foreach (var getterName in new[]
            {
                "PUB_SCENE_RENDER_PRIVATE", "PUB_SCENE_ENV_PRIVATE", "PUB_SCENE_DEBUG",
            })
            {
                string[] parts = GetTopic(t, getterName).Split('/');
                Assert.That(parts[(int)ArenaTopicTokens.TO_UID], Is.EqualTo("-"), getterName);
            }
        }

        [Test]
        public void SceneMsgTypes_AreTheSingleLetterAlphabetSharedWithArenaPy()
        {
            Assert.Multiple(() =>
            {
                Assert.That(ArenaTopicSceneMsgTypes.PRESENCE, Is.EqualTo("x"));
                Assert.That(ArenaTopicSceneMsgTypes.CHAT, Is.EqualTo("c"));
                Assert.That(ArenaTopicSceneMsgTypes.USER, Is.EqualTo("u"));
                Assert.That(ArenaTopicSceneMsgTypes.OBJECTS, Is.EqualTo("o"));
                Assert.That(ArenaTopicSceneMsgTypes.RENDER, Is.EqualTo("r"));
                Assert.That(ArenaTopicSceneMsgTypes.ENV, Is.EqualTo("e"));
                Assert.That(ArenaTopicSceneMsgTypes.PROGRAM, Is.EqualTo("p"));
                Assert.That(ArenaTopicSceneMsgTypes.DEBUG, Is.EqualTo("d"));
            });
        }

        [Test]
        public void TopicTokenIndices_AreStable()
        {
            Assert.Multiple(() =>
            {
                Assert.That((int)ArenaTopicTokens.REALM, Is.EqualTo(0));
                Assert.That((int)ArenaTopicTokens.TYPE, Is.EqualTo(1));
                Assert.That((int)ArenaTopicTokens.NAMESPACE, Is.EqualTo(2));
                Assert.That((int)ArenaTopicTokens.SCENENAME, Is.EqualTo(3));
                Assert.That((int)ArenaTopicTokens.SCENE_MSGTYPE, Is.EqualTo(4));
                Assert.That((int)ArenaTopicTokens.USER_CLIENT, Is.EqualTo(5));
                Assert.That((int)ArenaTopicTokens.UUID, Is.EqualTo(6));
                Assert.That((int)ArenaTopicTokens.TO_UID, Is.EqualTo(7));
            });
        }

        /// <summary>
        /// The parameterless struct constructor does NOT run the all-optional-argument
        /// constructor above - C# zero-initializes the struct instead - so every
        /// property comes back null rather than "". The topic strings are still
        /// well-shaped because string interpolation renders null as empty, but any
        /// caller that touches a segment directly (e.g. topics.nameSpace.Length) on a
        /// default-constructed value will throw.
        ///
        /// PINS CURRENT BEHAVIOUR (trap): Runtime/ArenaTopics.cs:39-52 - the
        /// constructor's default arguments are unreachable via `new ArenaTopics()` or
        /// `default(ArenaTopics)`. If the intent is that segments are never null, the
        /// properties need explicit `?? string.Empty` coalescing (or callers must
        /// always use the argument-taking constructor); the three Is.Null assertions
        /// below would then flip to Is.Empty.
        /// </summary>
        [Test]
        public void DefaultConstructor_LeavesSegmentsNullButStillFormsWellShapedTopics()
        {
            var t = new ArenaTopics();
            Assert.Multiple(() =>
            {
                Assert.That(t.REALM, Is.Null);
                Assert.That(t.nameSpace, Is.Null);
                Assert.That(t.sceneName, Is.Null);

                // Interpolation renders null as empty, so the shapes stay valid.
                Assert.That(t.SUB_NETWORK, Is.EqualTo("$NETWORK"));
                Assert.That(t.PUB_NETWORK_LATENCY, Is.EqualTo("$NETWORK/latency"));
                Assert.That(t.SUB_SCENE_PUBLIC, Is.EqualTo("/s///+/+/+"));
                Assert.That(t.PUB_SCENE_OBJECTS, Is.EqualTo("/s///o//"));
                Assert.That(t.PUB_SCENE_DEBUG, Is.EqualTo("/s///d///-"));
            });
        }

        /// <summary>
        /// Explicitly passing empty strings - or relying on the constructor's declared
        /// defaults by naming no arguments at all on the argument-taking overload -
        /// does give empty segments. This is the difference from the test above.
        /// </summary>
        [Test]
        public void ArgumentTakingConstructor_DefaultsSegmentsToEmptyStrings()
        {
            var t = new ArenaTopics(realm: "");
            Assert.Multiple(() =>
            {
                Assert.That(t.REALM, Is.Empty);
                Assert.That(t.nameSpace, Is.Empty);
                Assert.That(t.sceneName, Is.Empty);
                Assert.That(t.deviceName, Is.Empty);
                Assert.That(t.SUB_SCENE_PUBLIC, Is.EqualTo("/s///+/+/+"));
            });
        }
    }
}
