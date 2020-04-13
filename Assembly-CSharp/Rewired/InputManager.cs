using Rewired.Platforms;
using Rewired.Utils;
using Rewired.Utils.Interfaces;
using System.ComponentModel;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Rewired
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class InputManager : InputManager_Base
    {
        public InputManager()
            : this()
        {
        }

        protected override void DetectPlatform()
        {
            //IL_0002: Unknown result type (might be due to invalid IL or missing references)
            //IL_0009: Unknown result type (might be due to invalid IL or missing references)
            //IL_0010: Unknown result type (might be due to invalid IL or missing references)
            //IL_0042: Unknown result type (might be due to invalid IL or missing references)
            base.editorPlatform = (EditorPlatform)0;
            base.platform = (Platform)0;
            base.webplayerPlatform = (WebplayerPlatform)0;
            base.isEditor = false;
            string text = SystemInfo.deviceName ?? string.Empty;
            string text2 = SystemInfo.deviceModel ?? string.Empty;
            base.platform = (Platform)1;
        }

        protected override void CheckRecompile()
        {
        }

        protected override string GetFocusedEditorWindowTitle()
        {
            return string.Empty;
        }

        protected override IExternalTools GetExternalTools()
        {
            return (IExternalTools)(object)new ExternalTools();
        }

        private bool CheckDeviceName(string searchPattern, string deviceName, string deviceModel)
        {
            return Regex.IsMatch(deviceName, searchPattern, RegexOptions.IgnoreCase) || Regex.IsMatch(deviceModel, searchPattern, RegexOptions.IgnoreCase);
        }
    }
}
