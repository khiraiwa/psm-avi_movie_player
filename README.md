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
    　└lib
    　　└SampleLib

Finally, run avi_movie_player.sln and build this project.
