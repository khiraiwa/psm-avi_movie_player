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
 * This class is entry point.
 */
public class AppMain
{
    private static Movie movie;
    private static MoviePlayer player;
    private static GraphicsContext sm_GraphicsContext = null;

    static bool loop = true;

    public static void Main(string[] args)
    {

        Init();
        while (loop) {
            SystemEvents.CheckEvents();
            Update();
            Render();
        }
        Term();
    }

    static void InitUI(MoviePlayer player)
    {
        UISystem.Initialize(sm_GraphicsContext);
        Scene scene = new MoviePlayerScene(player);
        UISystem.SetScene(scene);
    }

    static void TermUI()
    {

    }

    public static void InitGraphicsContext()
    {
        if (null != sm_GraphicsContext) {
            return;
        }
        sm_GraphicsContext = new GraphicsContext();
    }

    public static void TermGraphicsContext()
    {
        if (null == sm_GraphicsContext) {
            return;
        }

        sm_GraphicsContext.Dispose();
        sm_GraphicsContext = null;
    }

    public static bool Init()
    {
        InitGraphicsContext();

        movie = new Movie();
        movie.Init(sm_GraphicsContext);
        player = movie.CreatePlayer();

        InitUI(player);

        return true;
    }

    public static void Term()
    {
        movie.Term();
        TermUI();
        TermGraphicsContext();
    }

    public static bool Update()
    {
        movie.Update();
        UpdateUI();
        return true;
    }

    public static void UpdateUI() {
        List<TouchData> touchDataList = Touch.GetData(0);
        UISystem.Update(touchDataList);
    }

    public static void Render() {
        if (null == sm_GraphicsContext) {
            return;
        }

        RenderBackground();
        movie.Render();
        RenderUI();
        sm_GraphicsContext.SwapBuffers();

    }

    public static void RenderBackground()
    {
        if (null == sm_GraphicsContext) {
            return;
        }

        sm_GraphicsContext.SetClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        sm_GraphicsContext.Clear();
    }

    public static void RenderUI()
    {
        UISystem.Render();
    }

    public static Movie GetMovie()
    {
        return movie;
    }
}

} // AppMain
