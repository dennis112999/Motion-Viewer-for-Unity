using System.IO;
using System;
using UnityEngine;

namespace Dennis.Tools.MotionViewer
{
    /// <summary>
    /// Screenshot Capturer Tool
    /// </summary>
    public static class ScreenshotCapturer
    {
        /// <summary>
        /// Captures and saves a Texture2D to disk as an image file
        /// </summary>
        /// <param name="savePath">Folder path to save the screenshot</param>
        /// <param name="screenshotTexture">Texture2D to save</param>
        /// <returns>full file path of the saved image</returns>
        public static string Capture(string savePath, Texture2D screenshotTexture)
        {
            // Encode the texture into the selected image format (PNG, JPG, EXR)
            byte[] imageData = EncodeScreenshot(screenshotTexture, out string extension);

            // Generate a unique file path and write the image to disk
            string filePath = GenerateScreenshotName(savePath, extension);
            File.WriteAllBytes(filePath, imageData);

            return filePath;

        }

        /// <summary>
        /// Encodes the Texture2D into PNG format
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="extension"></param>
        /// <returns>Encoded byte array of the image</returns>
        private static byte[] EncodeScreenshot(Texture2D texture, out string extension)
        {
            extension = ".png";
            return texture.EncodeToPNG();
        }

        /// <summary>
        /// Generates a timestamped file name for the screenshot
        /// </summary>
        /// <param name="savePath">Save folder path</param>
        /// <param name="extension">File extension (e.g. ".png")</param>
        /// <returns>Complete file path with name and extension</returns>
        private static string GenerateScreenshotName(string savePath, string extension)
        {
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            return Path.Combine(savePath, $"screenshot_{timeStamp}{extension}");
        }

        /// <summary>
        /// Converts a RenderTexture into a Texture2D
        /// </summary>
        /// <param name="rt">Source RenderTexture</param>
        /// <returns>A new Texture2D containing the RenderTexture content</returns>
        public static Texture2D RenderTextureToTexture2D(RenderTexture rt)
        {
            if (rt == null) return null;

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;

            // Create a new Texture2D and read pixels from the RenderTexture
            // Source: https://discussions.unity.com/t/rendertexture-readpixels-getpixel-shortcut/839338/2
            // Learned from Unity Forums discussion on reading RenderTexture into Texture2D
            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            RenderTexture.active = prev;

            return tex;
        }

    }
}
