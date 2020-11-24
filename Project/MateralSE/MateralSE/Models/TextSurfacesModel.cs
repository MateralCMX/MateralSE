using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace MateralSE.Models
{
    /// <summary>
    /// 文本面板模型
    /// </summary>
    public class TextSurfacesModel
    {
        private readonly ICollection<IMyTextSurface> _textSurfaces;
        public TextSurfacesModel(ICollection<IMyTextSurface> textSurfaces)
        {
            _textSurfaces = textSurfaces;
        }
        public void WriteText(string text)
        {
            foreach (IMyTextSurface textSurface in _textSurfaces)
            {
                textSurface.WriteText(text);
            }
        }
        /// <summary>
        /// 初始化主要文本面板
        /// </summary>
        /// <param name="mainTextSurfaceNames"></param>
        /// <param name="myGridTerminalSystem"></param>
        /// <param name="me"></param>
        public static TextSurfacesModel InitTextSurface(IReadOnlyCollection<string> mainTextSurfaceNames, IMyGridTerminalSystem myGridTerminalSystem, IMyProgrammableBlock me)
        {
            var textSurfaces = new List<IMyTextSurface>();
            foreach (string mainTextSurfaceName in mainTextSurfaceNames)
            {
                string[] trueNames = mainTextSurfaceName.Split('&');
                if (trueNames.Length == 1)
                {
                    var textSurface = myGridTerminalSystem.GetBlockWithName(trueNames[0]) as IMyTextSurface;
                    textSurfaces.Add(textSurface);
                }
                else if (trueNames.Length == 2)
                {
                    var index = Convert.ToInt32(trueNames[1]);
                    var textSurfaceProvider = myGridTerminalSystem.GetBlockWithName(trueNames[0]) as IMyTextSurfaceProvider;
                    if (textSurfaceProvider != null && textSurfaceProvider.SurfaceCount >= index)
                    {
                        textSurfaces.Add(textSurfaceProvider.GetSurface(index));
                    }
                }
            }
            textSurfaces.Add(me.GetSurface(0));
            return new TextSurfacesModel(textSurfaces);
        }
    }
}
