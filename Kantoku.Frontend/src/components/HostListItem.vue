<template lang="pug">
router-link(:to="'/dashboard/' + host")
    .unstyled.button.is-loading(v-if="!info")

    span(style="margin-right:auto") {{info?.hostName ?? host}}

    button.button(@click.stop.prevent.capture="remove" style="z-index:10000")
        icon(icon="trash")
</template>

<script lang="ts">
import { defineComponent, onUnmounted, ref } from "vue"

type HostInfo = {
    hostName: string
}

function sleep(ms: number) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

export default defineComponent({
    props: {
        host: String
    },
    emits: ["remove"],
    setup(props, { emit }) {
        const info = ref<HostInfo | null>(null);

        var exit = false;
        onUnmounted(() => exit = true);

        async function fetchInfo() {
            while (!exit) {
                var nextInterval = 10000;
            
                try {
                    const resp = await fetch(`http://${props.host}/info`)
                    const data = await resp.json();

                    info.value = data;
                } catch (e) {
                    nextInterval = 1000;
                    info.value = null;
                }

                await sleep(nextInterval);
            }
        }
        fetchInfo();

        function remove() {
            emit("remove");
        }

        return { remove, info }
    }
})
</script>

<style lang="scss" scoped>
.button.unstyled {
    background-color: unset;
    border: none;
}
</style>