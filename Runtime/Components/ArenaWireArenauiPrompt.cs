/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2023, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity
{
    public class ArenaWireArenauiPrompt : ArenaComponent
    {
        // ARENA arenaui-prompt component unity conversion status:
        // TODO: object_type
        // TODO: title
        // TODO: description
        // TODO: buttons
        // TODO: width
        // TODO: font
        // TODO: theme
        // TODO: materialSides

        public ArenaArenauiPromptJson json = new ArenaArenauiPromptJson();

        protected override void ApplyRender()
        {
            // TODO: Implement this component if needed, or note our reasons for not rendering or controlling here.
            Debug.Log("UI Prompt!");


            //1. create canvas
            //2. create Title
            //3. create Description
            //4. create Buttons (Confirm / Cancel)
        }

        public override void UpdateObject()
        {
            var newJson = JsonConvert.SerializeObject(json);
            if (updatedJson != newJson)
            {
                var aobj = GetComponent<ArenaObject>();
                if (aobj != null)
                {
                    aobj.PublishUpdate($"{newJson}");
                    apply = true;
                }
            }
            updatedJson = newJson;
        }
    }
}
