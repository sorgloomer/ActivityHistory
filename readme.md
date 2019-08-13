Activity History for Windows
============================


This is a windows forms application that logs the currently focused window in
a csv file. I use it to help me remember what I did on the computer at the end
of the day.

![Screenshot](/docs/screenshot1.png)


How does it work
----------------

You run the application manually, and it immediately starts recording the
focused windows title, executable and hwnd. It stores the data in a file called
`activity.log.csv` besides the executable.

It works by polling the currently focused window every 100ms, and records if
there is a change.