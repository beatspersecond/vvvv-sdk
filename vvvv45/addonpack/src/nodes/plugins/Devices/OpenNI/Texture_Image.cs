﻿#region usings
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Runtime.InteropServices;


using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2.EX9;

using OpenNI;
using SlimDX.Direct3D9;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;




#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "RGB",
                Category = "EX9.Texture",
                Version = "Kinect",
                Help = "Gesture recognition from the Kinect",
                Tags = "Kinect, OpenNI,",
                Author = "Phlegma")]
    #endregion PluginInfo


    public class Texture_Image : DXTextureOutPluginBase, IPluginEvaluate, IDisposable
    {
        #region fields & pins
        [Input("Context", IsSingle = true)]
        ISpread<Context> FContextIn;

        [Input("Enable", IsSingle = true, DefaultValue = 1)]
        IDiffSpread<bool> FEnableIn;

        [Import()]
        ILogger FLogger;

        private ImageGenerator FImageGenerator;

        private int FTexWidth;
        private int FTexHeight;

        private bool disposed = false;
        private bool FInit = true;
        #endregion fields & pins

        // import host and hand it to base constructor
        [ImportingConstructor()]
        public Texture_Image(IPluginHost host): base(host)
        {
        }

        #region Evaluate

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FContextIn[0] != null)
            {
                if (FInit == true)
                {
                    //Create an Image Generator
                    try
                    {
                        FImageGenerator = new ImageGenerator(FContextIn[0]);
                        ImageMetaData ImageMetaData = FImageGenerator.GetMetaData();

                        FTexWidth = ImageMetaData.XRes;
                        FTexHeight = ImageMetaData.YRes;
                        
                        //Reinitalie the vvvv texture
                        Reinitialize();

                        FInit = false;
                    }
                    catch (StatusException ex)
                    {
                        FLogger.Log(ex);
                    }
                }
            }
            else
            {
                FInit = true;
            }

            if (FEnableIn[0] == true)
                Update();
        }

        #endregion


        #region Dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.
                
                disposed = true;
            }
        }

        ~Texture_Image()
        {
            Dispose(false);
        }

        #endregion 


        #region IPluginDXTexture Members

        //this method gets called, when Reinitialize() was called in evaluate,
        //or a graphics device asks for its data
        protected override Texture CreateTexture(int Slice, SlimDX.Direct3D9.Device device)
        {
            return new Texture(device, FTexWidth, FTexHeight, 1, Usage.None, Format.X8R8G8B8, Pool.Managed);
        }

        //this method gets called, when Update() was called in evaluate,
        //or a graphics device asks for its texture, here you fill the texture with the actual data
        //this is called for each renderer, careful here with multiscreen setups, in that case
        //calculate the pixels in evaluate and just copy the data to the device texture here
        unsafe protected override void UpdateTexture(int Slice, Texture texture)
        {
            //lock the vvvv texture
            byte* dest32 = (byte*)texture.LockRectangle(0, LockFlags.Discard).Data.DataPointer;
                    
            //get the pointer to the Rgb Image
            byte* src24 = (byte*)FImageGenerator.ImageMapPtr;

            //write the pixels
            for (int i = 0; i < FTexWidth * FTexHeight; i++, src24 += 3, dest32 += 4)
            {
                dest32[0] = src24[2];
                dest32[1] = src24[1];
                dest32[2] = src24[0];
                dest32[3] = 255;
            }
            
            texture.UnlockRectangle(0);
        }

        #endregion IPluginDXResource Members
    }
}
