# Emotions
This is emotion recognizing tool for RGB-D camera (Kinect). It was my graduate project in 2015. Its main purpose is to provide facial features recording with MS Kinect during the special experiment. It comes with recording and playback module wich tracks change of facial parameters along with experiment state. At the moment, most of the data analysis is not yet implemented in tool itself but you can check the approach by looking at 'matlab' folder. But first, checkout the [slides](/doc/slides.pdf).

![Screenshot. Kinect Color View](/doc/screenshots/screenshot_view_color.png = 250x) ![Screenshot. Kinect Depth View](/doc/screenshots/screenshot_view_depth.png = 250x)



# Structure
 * `doc` - folder containing slides, screenshots and some sample data
 * `Emotions` - the main C# WPF project of the tool.
 * `Emotions.KinectTools` - library for recording kinect data. Also contains some useful classes for face tracking.
 * `External` - external libraries (.dll) which are required for proper working of some audio/video encoding.
 * `Microsoft.Kinect.Toolkit.FaceTracking` - fork of a Microsoft project from Kinect SDK with some changes in code. In order to run the project you will need to reference this version of `Microsoft.Kinect.Toolkit.FaceTracking` not the one from SDK.


# Experiment
This experiment was designed to detect correlation between human stress level and human facial expression. So I developed a some sort of game in which participant has to perform _tasks_ like clicking on the objects of one specific type e.g. blue circles. Experiment lasts fixed time (3 minutes). And there were same experiment conditions for all (everyone had the same circles at the same time).

![Screenshot. Game view](/doc/screenshots/screenshot_game.png)

_Tasks_ appears faster over time dividing game in three modes (according to model proposed by Arthur Siegel and James Wolf):

* Easy mode. In which participant performs 100% of tasks.
* Concentrated mode. In which participant can make mistakes but he increases his concentration on performing tasks.
* Hard mode. In which participant cannot perform tasks before new tasks appears which causes increase of a stress level.

And we obtained results like this:

![Sample experiment results](/doc/sample_results/sample_results_1_game.png)

And as result we are extracting 2 points from result:
*   Transition between relaxed (normal) state to concentrated state (by series of failures)
*   Transition between concentrated state to stressed state (by second series of failures)

![Sample experiment results](/doc/sample_results/sample_results_1_game_divided.png)

Along with that change of facial paramaters (Action Units) weere recorder (with same horizontal axis)

![Sample experiment results](/doc/sample_results/sample_results_1_game_action_units.png.png)

Combining it all together we get a set of training data for each person. Where objects are sets of values of action units `x = (au1, au2, au3, au4, au5, au6)` and labels are stress state class e.g. relaxed, concentrated or stressed.
But before we trained a model we ensured that facial expressions can really be classified by stress level, we got results like this for each person. _each dot (after factor analysis) represents a facial expression while its color states for our stress level label  e.g. relaxed, concentrated or stressed._

![Factor analysis](/doc/sample_results/sample_results_1_factor_analysis.png)

You can clearly see that it could be classified pretty much yeasily. I've already tried classification with neural networks and got pretty nice results.

For more information checkout the [slides](/doc/slides.pdf). And take a look at [matlab implementation of a classification](/matlab/).

# Requirements to run
First of all you need Microsoft Kinect v1 and its SDK:
 * [Kinect for Windows SDK 1.8](http://www.microsoft.com/en-us/download/details.aspx?id=40278)
 * [Kinect for Windows Developer Toolkit v1.8](http://www.microsoft.com/en-us/download/details.aspx?id=40276)

The tool is build in C# (WPF) so, unfortunately, it can only run under Windows.
The project written in Visual Studio 2013, but it can be simply upgraded to VS2015.





