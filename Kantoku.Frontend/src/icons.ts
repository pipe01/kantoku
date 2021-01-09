import { library } from "@fortawesome/fontawesome-svg-core"
import { faBackward, faPlay, faPause, faForward } from "@fortawesome/free-solid-svg-icons"
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'

library.add(faBackward, faPlay, faPause, faForward);

import { App } from 'vue';

export default {
    install(app: App) {
        app.component("icon", FontAwesomeIcon);
    }
}