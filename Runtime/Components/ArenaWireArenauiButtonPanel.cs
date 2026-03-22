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
    public class ArenaWireArenauiButtonPanel : ArenaComponent
    {
        // ARENA arenaui-button-panel component unity conversion status:
        // TODO: buttons
        // TODO: title
        // TODO: vertical
        // TODO: font
        // TODO: theme
        // TODO: materialSides

        public ArenaArenauiButtonPanelJson json = new ArenaArenauiButtonPanelJson();

        protected override void ApplyRender()
        {
            // TODO: Implement this component if needed, or note our reasons for not rendering or controlling here.
            //Debug.Log("UI Button Panel!");
        }

        public override void UpdateObject()
        {
            PublishIfChanged(JsonConvert.SerializeObject(json));
        }
    }
}
