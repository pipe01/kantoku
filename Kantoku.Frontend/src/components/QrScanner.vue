<template lang="pug">
canvas(hidden ref="canvasElement")
video(playsinline="true" ref="video")
</template>

<script lang="ts">
import { defineComponent, onMounted, onUnmounted, ref } from "vue"
import jsQR from "jsqr";

const hostRegex = /^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?):\d{1,5}$/

export default defineComponent({
    emits: [
        "found"
    ],
    setup(_, {emit}) {
        const hasVideo = ref(false);
        const canvasElement = ref<HTMLCanvasElement | null>(null);
        const video = ref<HTMLVideoElement | null>(null);
        const exit = ref(false);

        var stream: MediaStream | null

        function tick(canvas: CanvasRenderingContext2D) {
            if (video.value && video.value?.readyState == video.value?.HAVE_ENOUGH_DATA) {
                const w = canvas.canvas.height = video.value.videoHeight;
                const h = canvas.canvas.width = video.value.videoWidth;

                canvas.drawImage(video.value, 0, 0, w, h);
                const imageData = canvas.getImageData(0, 0, w, h);

                var code = jsQR(imageData.data, imageData.width, imageData.height, {
                    inversionAttempts: "dontInvert"
                });
                if (code && hostRegex.test(code.data)) {
                    emit("found", code.data);
                }
            }

            if (!exit.value) {
                requestAnimationFrame(() => tick(canvas));
            }
        }

        function onHashChanged(ev: HashChangeEventInit) {
            if (ev.newURL && new URL(ev.newURL).hash === "") {
                emit("found", null);
            }
        }

        onMounted(async () =>{
            if (!canvasElement.value || !video.value) {
                return;
            }

            window.addEventListener("hashchange", onHashChanged)
            location.hash = "#scan";

            const canvas = canvasElement.value.getContext("2d")!;

            stream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: "environment" } });
            video.value.muted = true;
            video.value.srcObject = stream;
            video.value.play();
            requestAnimationFrame(() => tick(canvas));
        });

        onUnmounted(() => {
            exit.value = true;

            window.removeEventListener("hashchange", onHashChanged)
            stream?.getVideoTracks().forEach(o => o.stop());
        });

        return { video, canvasElement }
    }
})
</script>
