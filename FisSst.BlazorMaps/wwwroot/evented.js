export function onCallback(dotnetHelper, evented, eventType) {
    evented.on(eventType, (mouseEvent) => {
        dotnetHelper.invokeMethodAsync('OnCallback', eventType, {
            type: mouseEvent.type,
            latLng: mouseEvent.latlng,
            page: {
                x: mouseEvent.originalEvent.pageX,
                y: mouseEvent.originalEvent.pageY
            },
            button: mouseEvent.originalEvent.button
        });        
    });
}

export function onCallbackEvent(dotnetHelper, evented, eventType) {
    evented.on(eventType, (e) => {
        dotnetHelper.invokeMethodAsync('OnCallbackEvent', eventType, {
            type: e.type
        });
    });
}