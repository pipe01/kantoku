<template lang="pug">
Dashboard
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import Dashboard from './components/Dashboard.vue'

export default defineComponent({
    name: 'App',
    components: {
        Dashboard
    }
})
</script>

<style lang="scss">
@charset "utf-8";
@import "node_modules/bulma/bulma";
</style>