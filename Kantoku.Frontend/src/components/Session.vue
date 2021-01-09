<template lang="pug">
.is-flex.is-flex-direction-column.is-align-items-center
    figure.image.is-96x96
        img.is-rounded(src="https://bulma.io/images/placeholders/128x128.png")
    
    span.mt-5 1:25 / 3:00
    input.slider.is-fullwidth.is-small.is-circle.mt-0(type="range" step="1" min="0" max="1000" value="25")

    .controls
        button.button.is-large.control X
        button.button.is-large.control X
        button.button.is-large.control X
</template>

<style lang="scss" scoped>
@import "node_modules/bulma/sass/helpers/flexbox";

.controls {
    display: flex;

    .control {
        border-radius: 1000000px !important;

        &:not(:last-child) {
            margin-right: 1rem;
        }
    }
}
</style>