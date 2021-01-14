<template lang="pug">
.container-fluid.h-100.is-flex.is-flex-direction-column
    .hero.is-info
        .hero-body
            span.title.has-text-light.has-text-weight-light {{isAdding ? "Scan host QR code" : "Connect to host"}}

    .p-3.list-container
        nav.panel.is-flex-grow-1.mb-5(v-if="!isAdding")
            p.panel-heading Known hosts

            router-link.panel-block(v-for="(host, i) in hosts" :to="'/dashboard/' + host")
                span(style="margin-right:auto") {{host}}
                button.button(@click.stop="remove(i)")
                    icon(icon="trash")

            a.panel-block.is-unselectable(@click="showScanner")
                span.panel-icon
                    icon(icon="plus")
                | Add new
</template>

<script lang="ts">
import { defineComponent, inject, reactive, ref } from "vue"
import { WebStorage } from "vue-web-storage";

const hostRegex = /^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?):\d{1,5}$/

export default defineComponent({
    emits: [ "selectedHost" ],
    setup(_, {emit}) {
        const isAdding = ref(false);
        
        const storage = inject<WebStorage>("storage")!
        const hosts = reactive(storage.get<string[]>("hosts") ?? []);

        function save() {
            storage.set("hosts", hosts);
        }

        function remove(i: number) {
            hosts.splice(i, 1);
            save();
        }

        function addHost(host: string | null) {
            isAdding.value = false;

            if (!host) {
                return;
            }

            if (hosts.includes(host)) {
                alert("That host is already on the list!");
            } else {
                hosts.push(host);
                save();
            }
        }

        function useHost(host: string) {
            emit("selectedHost", host);
        }

        function showScanner() {
            if (!window.cordova && process.env.NODE_ENV == "development") {
                addHost("192.168.1.33:4545")
            } else {
                cordova.plugins.barcodeScanner.scan(o => {
                    if (!o.cancelled) {
                        if (hostRegex.test(o.text)) {
                            addHost(o.text);
                        } else {
                            alert("That's not a valid Kantoku QR code");
                        }
                    }
                });
            }
        }

        return { hosts, remove, useHost, isAdding, addHost, showScanner }
    }
})
</script>

<style lang="scss" scoped>
.list-container {
    overflow-y: scroll;
}
</style>