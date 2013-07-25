/* PlayStation(R)Mobile SDK 1.11.01
 * Copyright (C) 2013 Sony Computer Entertainment Inc.
 * All Rights Reserved.
 */
using System;
using Sce.PlayStation.Core.Imaging;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Text;
using Sce.PlayStation.Core;
using Sce.PlayStation.Core.Graphics;
using Sce.PlayStation.Core.Environment;
using Sce.PlayStation.Core.Audio;
using Sce.PlayStation.HighLevel.UI;
using Sce.PlayStation.Core.Input;
using System.Net;

namespace Avi_Movie_Player
{

/**
 * エントリーポイントとなるクラス
 */
public static class PlayerRunner
{
    private static Movie movie;
    private static GraphicsContext sm_GraphicsContext = null;

    static bool loop = true;

    public static void Main(string[] args) {

        Init();
        movie.Play();
        while (loop) {
            SystemEvents.CheckEvents();
            Update();
            Render();
        }
        Term();
    }



    ////////////////////////////////////////////////////////////////
    static void InitUI() {
        UISystem.Initialize(sm_GraphicsContext);
        Scene scene = new MoviePlayerScene();
        UISystem.SetScene(scene);
    }
    static void TermUI() {

    }
    ////////////////////////////////////////////////////////////////

    static void InitGraphicsContext() {
        if (null != sm_GraphicsContext) {
            return;
        }
        sm_GraphicsContext = new GraphicsContext();
    }

    static void TermGraphicsContext() {
        if (null == sm_GraphicsContext) {
            return;
        }

        sm_GraphicsContext.Dispose();
        sm_GraphicsContext = null;
    }

    public static bool Init()
    {
        movie = new Movie(new Uri("http://localhost/sample.avi"));
        InitGraphicsContext();
        if (null != movie) {
            movie.Init(sm_GraphicsContext);
        }
        InitUI();

        return true;
    }

    /// Terminate
    public static void Term()
    {
        TermUI();
        TermGraphicsContext();
    }

    public static bool Update()
    {
        if (null != movie) {
            movie.Update();
        }
        UpdateUI();
        return true;
    }

    static void UpdateUI() {
        List<TouchData> touchDataList = Touch.GetData(0);
        UISystem.Update(touchDataList);
    }


    public static void Render() {
        if (null == sm_GraphicsContext) {
            return;
        }

        RenderBackground();
        if (null != movie) {
            movie.Render();
        }
        RenderUI();
        sm_GraphicsContext.SwapBuffers();

    }

    static void RenderBackground() {
        if (null == sm_GraphicsContext) {
            return;
        }

        sm_GraphicsContext.SetClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        sm_GraphicsContext.Clear();
    }

    static void RenderUI() {
        UISystem.Render();
    }

}

} // PlayerRunner