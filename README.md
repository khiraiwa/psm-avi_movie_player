psm-avi_movie_player
====================

## Introduction

Avi_movie_player is movie player for avi format.
Currently it supports the following video and audio type.

* Video encoding format  
  Motion JPEG
* Audio encoding format  
  MP3  

PSM SDK isn't supported playing movie.
So I created this program.

## Requirements
* PSM SDK 1.11.01
* PSM Development Assistant 1.11.01

## Build

First, clone this project.
Run the following code.

    $ git clone https://github.com/khiraiwa/psm-avi_movie_player.git

Second, copy SampleLib to this project root directory. 
This application is dependent on SampleLib project.
This project is involved in PSM SDK sample.

By default, SampleLib is installed on the following directory.

    C:\Users\Public\Documents\PSM\sample\lib

Copy this lib direcoty to project root direcoty as below.

    psm-avi_movie_player
    　├avi_movie_player.sln
    　├AppMain.cs
    　・
    　・
    　・
    　└lib
    　　└SampleLib

Finally, run avi_movie_player.sln by use of PsmStudio.
And build this project.

## Run

Set project of avi movie player for start up.
And run this solution using Playstation mobile Simulator or PS Vita, etc.

Then sample app is run.

You can use URI for location of avi movie.
Currently, this project supports scheme of http and file.

For example,

    http://127.0.0.1/output.avi
    file:///Documents/contents/output.avi

## Usage
The class of Avi_Movie_Player.Movie is important.

Create this class object.

    Movie movie = new Movie();
    
Initialize Movie class with an argument of Sce.PlayStation.Core.Graphics.GraphicsContext.

    movie.Init(graphicsContext);

Update this object.

    movie.Update();

Render  this object.

    movie.Render();

Term  this object.

    movie.Term();

Please see AppMain.cs.

## Appendix

### How to create avi movie file.

There are some way of creating avi movie files.
For one, I express the method using ffmpeg.
Run the following command.

    $ ffmpeg -i [input video] -vcodec mjpeg -acodec mp3 output.avi
