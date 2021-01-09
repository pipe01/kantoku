<template lang="pug">
section.is-flex.is-flex-direction-column.is-align-items-center.px-5
    figure.image.is-96x96
        img.is-rounded(v-if="session.app" :src="session.app.icon")
    
    span.is-size-4.has-text-centered.has-text-weight-bold.mt-5 {{session.media.author}}
    span.is-size-5.has-text-centered.mb-5 {{session.media.title}}
    input.slider.is-fullwidth.is-small.is-circle.my-1(type="range" step="1" min="0" max="1000" :value="sliderValue")
    span.mb-2 {{session.position}}

    .controls
        button.button.is-large
            icon(icon="backward")
        button.button.is-large
            icon(v-if="session.isPlaying" icon="pause")
            icon(v-else="" icon="play")
        button.button.is-large
            icon(icon="forward")
</template>

<script lang="ts">
import { computed, defineComponent, PropType } from 'vue'
import { Session } from "../models";

export default defineComponent({
    props: {
        session: {
            type: Object as PropType<Session>,
            required: true
        }
    },
    setup(props) {
        const sliderValue = computed(() => (props.session.position / props.session.media.duration) * 1000);

        return { sliderValue }
    }
})
</script>

<style lang="scss" scoped>
@import "node_modules/bulma/sass/helpers/flexbox";

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