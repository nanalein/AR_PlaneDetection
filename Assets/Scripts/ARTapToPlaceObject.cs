using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Experimental.XR;
using System;

public class ARTapToPlaceObject : MonoBehaviour
{
    public GameObject objectToPlace; // puclic GameObject legt fest welche GameObject gespawned werden soll
    public GameObject placementIndicator; // public GameObject welches in der Szene up-gedated wird (Position)
    private ARSessionOrigin arOrigin; // private Reference um mit dem ARSessionOrigin zu interagieren
    private Pose placementPose; // eine Pose Variable beschreibt die Position und Rotation eines 3D Punkts im Raum
    private bool placementPoseIsValid = false; // checkt ob Ergebniss aus arOrigin.Raycast eine Plane sieht oder keine
    public float hoehe = 0.1f;
    public float breite = 0.1f;
    public float tiefe = 0.1f;
    

    // Start is called before the first frame update
    void Start()
    {
        arOrigin = FindObjectOfType<ARSessionOrigin>(); // sobald Start wird eine Ref davon in arOrigin gespeichert
        objectToPlace.transform.localScale = new Vector3(breite, hoehe, tiefe);
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlacementPose(); // prüft jeden Frame wohin die Camera zeigt und ob ein Objekt dort platziert werden kann
        UpdatePlacementIndicator(); // wird genutzt um die Visuals up-zu-daten

        if (placementPoseIsValid && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) // prüft ob placementPoseIsValid && hat der User einen Finger auf dem Screen && checken ob die Touch-Phase dieses Fingers gerade begonnen hat
        {
            PlaceObject(); //wenn alle Bedingungen true dann platziere ein neues Objekt
        }
    }

    private void PlaceObject()
    {
        GameObject newObject = Instantiate(objectToPlace, placementPose.position, placementPose.rotation) as GameObject;
        newObject.transform.localScale = new Vector3(breite, hoehe, tiefe);
    }

    private void UpdatePlacementIndicator()
    {
        if (placementPoseIsValid)
        {
            placementIndicator.SetActive(true); // wenn placementPoseIsValid setzte das GameObject auf visible und...
            placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation); // ...setze das GameObject an die Position mit Rotation von placementPose
        }
        else
        {
            placementIndicator.SetActive(false); //wenn placementPoseIsValid nicht valid setzte das GameObject auf invisible
        }
    }

    private void UpdatePlacementPose()
    {
        var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f)); // bestimmt Mitte vom iPhone Screen
        var hits = new List<ARRaycastHit>(); // speichert alle Hit-Points vom Rayctrace
        arOrigin.Raycast(screenCenter, hits, TrackableType.Planes); // Raycast von mitte Screen, speichert in hits und tracked alle verfügbaren Planes
    
        placementPoseIsValid = hits.Count > 0; // setzt die bool auf true wenn mind. 1 plane getroffen wurde
        if (placementPoseIsValid)         // wenn placementPoseIsValid...
        {
            placementPose = hits[0].pose; // ...dann nimm die erste Hit-Result aus dem Array und lese die Pose-Werte aus

            var cameraForward = Camera.current.transform.forward; // ist ein Vector(x,y,z) der die Richtung erkennt wohin die Camera blickt (wie ein Pfeil)
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized; // nimmt nur die x- und z-werte aus cameraForward (y-interessiert nicht weil eh auf Boden) und normalisiert die Werte noch
            placementPose.rotation = Quaternion.LookRotation(cameraBearing); // nimmt Rotation aus cameraBearing und setzt diese in placementPose.totation
        }
    }
}
