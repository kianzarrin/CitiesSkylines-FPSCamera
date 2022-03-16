using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace FPSCamMod
{
    using CfKey = ConfigData<KeyCode>;

    public static class OptionsMenuUI
    {
        public static void Generate(UIHelperBase uiHelper)
        {
            helperPanel = (uiHelper as UIHelper).self as UIScrollablePanel;
            SetUp();
        }
        private static void SetUp()
        {
            var mainPanel = helperPanel.AsParent().AddGroup("First Person Camera");
            var mainParent = mainPanel.AsParent();
            mainPanel.backgroundSprite = "";
            const float margin = 5f;
            {
                var panel = mainParent.AddGroup("General Options");
                var parent = panel.AsParent();
                panel.autoLayout = false;
                UIComponent comp;
                var y = 0f;
                comp = parent.AddCheckbox(Config.G.UseMetricUnit, yPos: y);
                y += comp.height + margin;
                comp = parent.AddCheckbox(Config.G.InvertRotateVertical, yPos: y);
                y += comp.height + margin;
                comp = parent.AddCheckbox(Config.G.InvertRotateHorizontal, yPos: y);
                y += comp.height + margin;
                comp = parent.AddSlider(Config.G.RotateSensitivity, .25f,
                                         yPos: y, width: panel.width, oneLine: true);
                y += comp.height + margin;
                comp = parent.AddSlider(Config.G.MaxVertRotate, 1f, "F0",
                                         yPos: y, width: panel.width, oneLine: true);
                y += comp.height + margin;
                panel.height = y;
                parent.AddButton("ReloadConfig", "Reload Configurations", new Vector2(200f, 35f),
                                 (_, p) => { Mod.LoadConfig(); Mod.ResetUI(); },
                                 panel.width - 240f, 0f);
                parent.AddButton("ResetConfig", "Reset Configurations", new Vector2(200f, 35f),
                                 (_, p) => Mod.ResetConfig(), panel.width - 240f, 35f);
            }
            {
                var panel = mainParent.AddGroup("Free-Camera Mode Options");
                var parent = panel.AsParent();
                parent.AddCheckbox(Config.G.ShowCursorWhileFreeCam);
                parent.AddSlider(Config.G.GroundLevelOffset, .25f,
                                  width: panel.width, oneLine: true);
            }
            {
                var panel = mainParent.AddGroup("Follow/Walk-Through Mode Options");
                var parent = panel.AsParent();
                parent.AddCheckbox(Config.G.ShowCursorWhileFollow);
                parent.AddSlider(Config.G.MaxVertRotate4Follow, 1f, "F0",
                                  width: panel.width, oneLine: true);
                parent.AddSlider(Config.G.MaxHoriRotate4Follow, 1f, "F0",
                                  width: panel.width, oneLine: true);
                parent.AddSlider(Config.G.InstantMoveMax, 1f, "F0",
                                  width: panel.width, oneLine: true);
            }
            {
                var panel = mainParent.AddGroup("Key Mapping");
                var label = panel.AsParent().AddLabel("KeyMappingComment",
                                "* Press [ESC]: cancel |  * Press [Shift]+[X]: remove");
                panel.gameObject.AddComponent<KeyMappingUI>();
            }
            {
                var panel = mainParent.AddGroup("Smooth Transition Options");
                var parent = panel.AsParent();
                parent.AddSlider(Config.G.TransitionSpeed, 1f, "F0",
                                  width: panel.width, oneLine: true);
                parent.AddSlider(Config.G.GiveUpTransitionDistance, 50f, "F0",
                                  width: panel.width, oneLine: true);
                parent.AddSlider(Config.G.DeltaPosMin, .05f,
                                  width: panel.width, oneLine: true);
                parent.AddSlider(Config.G.DeltaPosMax, 1f, "F0",
                                  width: panel.width, oneLine: true);
                parent.AddSlider(Config.G.DeltaRotateMin, .05f,
                                  width: panel.width, oneLine: true);
                parent.AddSlider(Config.G.DeltaRotateMax, 1f, "F0",
                                  width: panel.width, oneLine: true);
            }
            {
                var panel = mainParent.AddGroup("Camera Offsets");
                var parent = panel.AsParent();
                parent.AddOffsetSliders(Config.G.VehicleCamOffset, width: panel.width);
                parent.AddOffsetSliders(Config.G.CitizenCamOffset, width: panel.width);
            }
        }
        public static void Destroy()
        {
            if (helperPanel != null) {
                var optionPanel = helperPanel.Find("OptionsGroupTemplate(Clone)");
                helperPanel.RemoveUIComponent(optionPanel);
                Object.Destroy(optionPanel);
            }
        }
        public static void Rebuild() { if (helperPanel != null) { Destroy(); SetUp(); } }

        private static UIScrollablePanel helperPanel;
    }

    public class KeyMappingUI : UICustomControl
    {
        private CfKey configWaiting;

        private void Awake()
        {
            AddKeyMapping(Config.G.KeyCamToggle);

            AddKeyMapping(Config.G.KeySpeedUp);
            AddKeyMapping(Config.G.KeyCamReset);
            AddKeyMapping(Config.G.KeyCursorToggle);

            AddKeyMapping(Config.G.KeyMoveForward);
            AddKeyMapping(Config.G.KeyMoveBackward);
            AddKeyMapping(Config.G.KeyMoveLeft);
            AddKeyMapping(Config.G.KeyMoveRight);
            AddKeyMapping(Config.G.KeyMoveUp);
            AddKeyMapping(Config.G.KeyMoveDown);

            AddKeyMapping(Config.G.KeyRotateUp);
            AddKeyMapping(Config.G.KeyRotateDown);
            AddKeyMapping(Config.G.KeyRotateLeft);
            AddKeyMapping(Config.G.KeyRotateRight);
        }

        private void AddKeyMapping(CfKey config)
        {
            var panel = component.AsParent().AddUI<UIPanel>("KeyBindingTemplate");

            var btn = panel.Find<UIButton>("Binding");
            btn.eventKeyDown += new KeyPressHandler(KeyPressAction);
            btn.eventMouseDown += new MouseEventHandler(MouseEventAction);
            btn.text = config.ToString();
            btn.textColor = UIutils.textColor;
            btn.objectUserData = config;

            var label = panel.Find<UILabel>("Name");
            label.text = config.Description; label.tooltip = config.Detail;
            label.textColor = UIutils.textColor;
        }

        private void KeyPressAction(UIComponent comp, UIKeyEventParameter p)
        {
            if (configWaiting is object) {
                p.Use();
                UIView.PopModal();

                var btn = p.source as UIButton;
                var key = p.keycode;
                if (p.shift && key == KeyCode.X) configWaiting.assign(KeyCode.None);
                else if (key != KeyCode.Escape) configWaiting.assign(key);

                btn.text = configWaiting.ToString();
                Config.G.Save();
                Log.Msg($"Config: assign \"{configWaiting}\" to [{configWaiting.Name}]");
                configWaiting = null;
            }
        }

        private void MouseEventAction(UIComponent comp, UIMouseEventParameter p)
        {
            if (configWaiting is null) {
                p.Use();

                var btn = p.source as UIButton;
                configWaiting = (CfKey) btn.objectUserData;

                btn.text = "(Press a key)";
                btn.Focus();
                UIView.PushModal(btn);
            }
        }
    }
}
