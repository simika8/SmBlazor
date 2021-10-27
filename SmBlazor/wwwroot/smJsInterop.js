// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

export function scrollToElement(element) {
    //console.info('scrollToElement: ', element);
    if (!element) {
        return false;
    }
    element.scrollIntoView({ behavior: "auto", block: "nearest" });
    return true;
}

export function clientHeight(element) {
    let res = element.clientHeight;
    //console.info('height: ', height);
    return res;
}

export function focusToElement(element) {
    //console.info('focusToElement: ', element);
    if (!element) {
        return false;
    }
    element.focus();
    return true;
}

export function subscribeToChange(componentRef, inputRef) {
    //console.log("subscribeToChange!");

    inputRef.onkeydown = function (event) {
        const processedKeys = ["ArrowUp", "ArrowDown", "Home", "End", "PageUp", "PageDown", "Insert", "+"];
        if (processedKeys.includes(event.code)) {
            //console.log("processed keydown event.keyCode :", event.keyCode, event.code);
            event.preventDefault();

            componentRef.invokeMethodAsync('keyboardEventHandlerFromJs', event.keyCode, event.code);
        }
    };

    inputRef.onkeyup = function (event) {
        componentRef.invokeMethodAsync('setValueFromJS', inputRef.value);
    };
}
