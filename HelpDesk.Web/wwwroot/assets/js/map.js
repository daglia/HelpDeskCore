//map.js

//Set up some of our variables.
var map; //Will contain map object.
var marker = false; ////Has the user plotted their location marker? 

//Function called to initialize / create the map.
//This is called when the page has loaded.
function initMap() {

    //The center location of our map.
    var centerOfMap = new google.maps.LatLng(41.0641, 28.9941);

    //Map options.
    var options = {
        center: centerOfMap, //Set center.
        zoom: 9 //The zoom value.
    };

    //Create the map object.
    map = new google.maps.Map(document.getElementById('map'), options);

    //Listen for any clicks on the map.
    google.maps.event.addListener(map, 'click', function (event) {
        //Get the location that the user clicked.
        var clickedLocation = event.latLng;
        //If the marker hasn't been added.
        if (marker === false) {
            //Create the marker.
            marker = new google.maps.Marker({
                position: clickedLocation,
                map: map,
                draggable: true //make it draggable
            });
            //Listen for drag events!
            google.maps.event.addListener(marker, 'dragend', function (event) {
                markerLocation();
            });
        } else {
            //Marker has already been added, so just change its location.
            marker.setPosition(clickedLocation);
        }
        //Get the marker's location.
        markerLocation();
    });
}

//This function will get the marker's current location and then add the lat/long
//values to our textfields so that we can save the location.
function markerLocation() {
    //Get location.
    var currentLocation = marker.getPosition();
    //Add lat and lng values to a field that we can save.
    document.getElementById('ltd').value = currentLocation.lat(); //latitude
    document.getElementById('lng').value = currentLocation.lng(); //longitude
    document.getElementById('lng').value = document.getElementById('lng').value.replace(".", ",");
    document.getElementById('ltd').value = document.getElementById('ltd').value.replace(".", ",");
}


//Load the map when the page has finished loading.
google.maps.event.addDomListener(window, 'load', initMap);

var x;
function getLocation() {
    x = document.getElementById("location");
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(putPosition);
    }
    else {
        x.innerHTML = "Geolocation is not supported by this browser.";
    }
}
function putPosition(position) {
    document.getElementById('ltd').value = position.coords.latitude; //latitude
    document.getElementById('lng').value = position.coords.longitude; //longitude
    document.getElementById('lng').value = document.getElementById('lng').value.replace(".", ",");
    document.getElementById('ltd').value = document.getElementById('ltd').value.replace(".", ",");
}