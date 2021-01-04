document.querySelectorAll("video").forEach(el => attachTo(<Element>el));

var observer = new MutationObserver((mutations) => {
    for (const mut of mutations) {
        if (mut.type == "childList")
            mut.addedNodes.forEach(el => attachTo(<Element>el));
    }
});

observer.observe(document, { attributes: false, childList: true, characterData: false, subtree:true });

function attachTo(element: Element) {
    if (element.tagName != "VIDEO")
        return;

    var video = <HTMLVideoElement>element

    video.addEventListener("pause", () => console.log("video paused"));
    video.addEventListener("play", () => console.log("video play"));
    video.addEventListener("timeupdate", () => console.log("video progress", video.currentTime));
    video.addEventListener("loadstart", () => console.log("load start"));
}
