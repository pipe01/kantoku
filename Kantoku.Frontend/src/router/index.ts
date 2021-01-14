import { createRouter, createWebHashHistory, RouteRecordRaw } from 'vue-router'
import HostSelection from '@/views/HostSelection.vue'
import Dashboard from '@/views/Dashboard.vue'

const routes: Array<RouteRecordRaw> = [
    {
        path: '/',
        name: 'Home',
        component: HostSelection
    },
    {
        path: '/dashboard/:host',
        name: 'Dashboard',
        component: Dashboard
    }
]

const router = createRouter({
    history: createWebHashHistory(),
    routes
})

export default router
