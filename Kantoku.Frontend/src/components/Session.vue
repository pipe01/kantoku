<template lang="pug">
section.is-flex.is-flex-direction-column.is-align-items-center.px-5
    figure.image.is-96x96
        img.is-rounded(v-if="session.app" :src="session.app.icon" :title="session.app.name")
    
    span.is-size-4.has-text-centered.has-text-weight-bold.mt-5 {{session.media.author}}
    span.is-size-5.has-text-centered.mb-5 {{session.media.title}}
    input.slider.is-fullwidth.is-small.is-circle.my-1(type="range" step="any" min="0" :max="session.media?.duration" v-model="position" @change="positionChanged")
    span.mb-2 {{formatTime(session.position)}} / {{formatTime(session.media?.duration)}}

    .controls
        button.button.is-large(@click="api.previous")
            icon(icon="backward")
            
        button.button.is-large(v-if="session.isPlaying" @click="api.pause")
            icon(icon="pause")
        button.button.is-large(v-else="" @click="api.play")
            icon(icon="play")

        button.button.is-large(@click="api.next")
            icon(icon="forward")
</template>

<script lang="ts">
import { computed, defineComponent, PropType } from 'vue'
import { useApi } from '@/api';
import { Session } from "@/models";

export default defineComponent({
    props: {
        session: {
            type: Object as PropType<Session>,
            required: true
        }
    },
    setup(props) {
        const api = useApi();

        const sessionApi = computed(() => api?.forSession(props.session.id));
        const position = computed({
            get: () => props.session.position,
            set: val => props.session.position = typeof val == "string" ? Number.parseFloat(val) : val
        })

        function formatTime(n: number) {
            const min = Math.floor(n / 60);
            const sec = Math.floor(n % 60);

            const format = (a: number) => a.toLocaleString(undefined, {minimumIntegerDigits: 2});

            return `${format(min)}:${format(sec)}`;
        }

        function positionChanged() {
            sessionApi.value?.setPosition(position.value);
        }

        return { api: sessionApi, formatTime, positionChanged, position }
    }
})
</script>

<style lang="scss" scoped>
section {
    width: 100vw;
}

.controls {
    display: flex;

    .button {
        border-radius: 10000px !important;
        color: gray;

        &:not(:last-child) {
            margin-right: 1rem;
        }
    }
}
</style>