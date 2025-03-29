using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;


public class AndroidImage : MonoBehaviour
{
   
    public string imagePath;
    public Sprite sprite;
    public void OpenGallery()
    {
        
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
           
            if (path != null)
            {
               
                 sprite = Sprite.Create(NativeGallery.LoadImageAtPath(path, 512, false), new Rect(0, 0, 16, 16), Vector2.zero);
               
                string destinationPath = Path.Combine(Application.persistentDataPath +"/image/", Path.GetFileName(path));
               
               
                File.Copy(path, destinationPath, true);
               
                // imagePath = Path.Combine(Application.persistentDataPath, Path.GetFileName(path));
                imagePath = destinationPath;
            }
        }, "Select a PNG image", "image/png");

        
    }
}
