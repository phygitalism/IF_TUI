# Recognition middleware for TUI

This project used to recognize tangible markers (with three fingers) by touchpoints. RecognitinService determines markers position and rotation angle. RecognitionGUI is used to register tangible markers and show them on the display. We used [pqlabs g5s frame](http://www.pqlabs.com/g5s-spec.html) and conventional display as a multitouch system, but you can use any other multitouch frame. Infrared frame detects touchpoints and sends it by [TUIO](https://www.tuio.org) protocol to our application.

![general](https://drive.google.com/uc?id=1rY9WG-Mdwl6JyIdGfSXr_vw4nPAZsuTs)

## Run
### RecognitionService
Build RecognitionService. Run **RecognitionService.exe**. Make sure it is launched in background mode. ![run](https://drive.google.com/uc?id=1ApZdckigLXwGp-wLo7U_6lJWT7vR6W96)
### RecognitionGUI
Build RecognitionGUI. Run **RecognitionGUI.exe**.

## Registration
The app wouldn't recognize and show your marker until you register it. To register marker, put it into center field and chose id number.

![registration_ui](https://drive.google.com/uc?id=17AXrFtqr6IVlkUCOsK3TQCSIjSnYcayr)

After that you can put the marker at any place of the multitouch surface and UI will show it.

![recognized_ui](https://drive.google.com/uc?id=1REigJyN-7-L79crRkGVspsq1GuCthWJE)

The information about marker parameters saves in **markers.json**.
Make sure that file **config.localstorage** exists and it’s not empty. If config.localstorage is empty, it’s necessary to register markers.

## UI
After you registered all needed markers, you can connect any available vizualisation which sends and receives TUIO data (cursors/objects) to and from TUIO server/client apps, like [this project](https://github.com/gregharding/TUIOSimulator).

Before it, check your TUIO sending and receiving ports.

![platform_ui](https://drive.google.com/uc?id=1rv14QcXfrRXfLxnwj9Ci-gIClVk2LSbC)

![example_ui](https://drive.google.com/uc?id=1040a4I7EsjRuk-_kbodMmCYOdqt4KkU2)

## The algorithm
The algorithm works with sets of touchpoints and their parameters (position, id, type, etc.), coming from an infrared frame.

The main idea is to compare triangles formed by touchpoints from the frame with pre-known registered marker-triangles. Parameters which describe unique triangles:

![triangle](https://drive.google.com/uc?id=1EWaHDKJc96O8Y_FNQD1xhPRBjzzeizFX)

- The length of the three sides;
- The triangle orientation (clockwise or counterclockwise).

Recognized triangles are followed by ID, so we take into account the case of loss of one or even two marker fingers.