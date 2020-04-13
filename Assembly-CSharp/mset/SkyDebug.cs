using UnityEngine;

namespace mset
{
    public class SkyDebug : MonoBehaviour
    {
        public bool printConstantly = true;

        public bool printOnce;

        public bool printToGUI = true;

        public bool printToConsole;

        public bool printInEditor = true;

        public bool printDetails;

        public string debugString = string.Empty;

        private MaterialPropertyBlock block;

        private GUIStyle debugStyle;

        private static int debugCounter;

        private int debugID;

        private void Start()
        {
            debugID = debugCounter;
            debugCounter++;
        }

        private void LateUpdate()
        {
            bool flag = printOnce || printConstantly;
            if ((bool)GetComponent<Renderer>() && flag)
            {
                printOnce = false;
                debugString = GetDebugString();
                if (printToConsole)
                {
                    Debug.Log(debugString);
                }
            }
        }

        public string GetDebugString()
        {
            string str = "<b>SkyDebug Info - " + base.name + "</b>\n";
            Material material = null;
            material = ((!Application.isPlaying) ? GetComponent<Renderer>().sharedMaterial : GetComponent<Renderer>().material);
            str = str + material.shader.name + "\n";
            string text = str;
            str = text + "is supported: " + material.shader.isSupported + "\n";
            ShaderIDs[] array = new ShaderIDs[2]
            {
                new ShaderIDs(),
                new ShaderIDs()
            };
            array[0].Link();
            array[1].Link("1");
            str += "\n<b>Anchor</b>\n";
            SkyAnchor component = GetComponent<SkyAnchor>();
            if (component != null)
            {
                str = str + "Curr. sky: " + component.CurrentSky.name + "\n";
                str = str + "Prev. sky: " + component.PreviousSky.name + "\n";
            }
            else
            {
                str += "none\n";
            }
            str += "\n<b>Property Block</b>\n";
            if (block == null)
            {
                block = new MaterialPropertyBlock();
            }
            block.Clear();
            GetComponent<Renderer>().GetPropertyBlock(block);
            for (int i = 0; i < 2; i++)
            {
                str = str + "Renderer Property block - blend ID " + i;
                if (printDetails)
                {
                    str = str + "\nexposureIBL  " + block.GetVector(array[i].exposureIBL);
                    str = str + "\nexposureLM   " + block.GetVector(array[i].exposureLM);
                    str = str + "\nskyMin       " + block.GetVector(array[i].skyMin);
                    str = str + "\nskyMax       " + block.GetVector(array[i].skyMax);
                    str += "\ndiffuse SH\n";
                    for (int j = 0; j < 4; j++)
                    {
                        str = str + block.GetVector(array[i].SH[j]) + "\n";
                    }
                    str += "...\n";
                }
                Texture texture = block.GetTexture(array[i].specCubeIBL);
                Texture texture2 = block.GetTexture(array[i].skyCubeIBL);
                str += "\nspecCubeIBL  ";
                str = ((!texture) ? (str + "none") : (str + texture.name));
                str += "\nskyCubeIBL   ";
                str = ((!texture2) ? (str + "none") : (str + texture2.name));
                if (printDetails)
                {
                    str = str + "\nskyMatrix\n" + block.GetMatrix(array[i].skyMatrix);
                    str = str + "\ninvSkyMatrix\n" + block.GetMatrix(array[i].invSkyMatrix);
                }
                if (i == 0)
                {
                    str = str + "\nblendWeightIBL " + block.GetFloat(array[i].blendWeightIBL);
                }
                str += "\n\n";
            }
            return str;
        }

        private void OnDrawGizmosSelected()
        {
            bool flag = printOnce || printConstantly;
            if ((bool)GetComponent<Renderer>() && printInEditor && printToConsole && flag)
            {
                printOnce = false;
                string message = GetDebugString();
                Debug.Log(message);
            }
        }

        private void OnGUI()
        {
            if (printToGUI)
            {
                Rect position = Rect.MinMaxRect(3f, 3f, 360f, 1024f);
                if ((bool)Camera.main)
                {
                    position.yMax = Camera.main.pixelHeight;
                }
                position.xMin += (float)debugID * position.width;
                GUI.color = Color.white;
                if (debugStyle == null)
                {
                    debugStyle = new GUIStyle();
                    debugStyle.richText = true;
                }
                string str = "<color=\"#000\">";
                string str2 = "</color>";
                GUI.TextArea(position, str + debugString + str2, debugStyle);
                str = "<color=\"#FFF\">";
                position.xMin -= 1f;
                position.yMin -= 2f;
                GUI.TextArea(position, str + debugString + str2, debugStyle);
            }
        }
    }
}
