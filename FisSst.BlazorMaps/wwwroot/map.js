export function initialize(mapOptions) {
    const newMap = L.map(mapOptions.divId).setView(mapOptions.center, mapOptions.zoom);
    L.tileLayer(mapOptions.urlTileLayer, mapOptions.subOptions).addTo(newMap);

    if (!mapOptions.zoomEnabled) {
        newMap.removeControl(newMap.zoomControl);
    }

    return newMap;
}