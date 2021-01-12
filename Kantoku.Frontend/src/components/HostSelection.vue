<template lang="pug">
.container-fluid.h-100.is-flex.is-flex-direction-column
    .hero.is-info
        .hero-body
            span.title.has-text-light.has-text-weight-light Connect to host

    nav.panel.is-flex-grow-1.p-3
        p.panel-heading Known hosts

        a.panel-block(v-for="(host, i) in hosts" @click.self="useHost(host)")
            | {{host}}
            button.button(@click="remove(i)")
                icon(icon="trash")

        a.panel-block.is-unselectable(@click="addNew")
            span.panel-icon
                icon(icon="plus")
            | Add new
</template>

<script lang="ts">
import { defineComponent, inject, reactive } from "vue"
import { WebStorage } from "vue-web-storage";

export default defineComponent({
    setup() {
        const storage = inject<WebStorage>("storage")!
        const hosts = reactive(storage.get<string[]>("hosts") ?? []);

        function save() {
            storage.set("hosts", hosts);
        }

        function addNew() {
            hosts.push(new Date().toString());
            save();
        }

        function remove(i: number) {
            hosts.splice(i, 1);
            save();
        }

        function useHost(host: string) {
            alert("Use: " + host);
        }

        return { hosts, addNew, remove, useHost }
    }
})
</script>