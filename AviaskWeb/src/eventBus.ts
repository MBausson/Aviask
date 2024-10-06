import { ref } from "vue";

const bus = ref<Map<string, string[]>>(new Map());

export default function useEventBus() {
    function emitBus(event: string, ...args: string[]) {
        bus.value.set(event, args);
    }

    return { emitBus, bus };
}
