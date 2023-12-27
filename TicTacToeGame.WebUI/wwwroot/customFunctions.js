window.registerEventHandler = function (eventName, dotNetHelper, methodName) {
    document.addEventListener(eventName, function (event) {
        
        if (!event.target.closest("#chatContainer") && event.target.id !== "chatButton") {
            dotNetHelper.invokeMethodAsync(methodName);
        }
    });
};
