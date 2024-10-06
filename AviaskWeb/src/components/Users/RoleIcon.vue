<script lang="ts" setup>
import { HeroIcon } from "@/components";
import { computed } from "vue";
import type { IconName } from "../Widgets/HeroIcon.vue";

const { role } = defineProps({
    role: {
        required: true,
        type: String,
    },
});

const roleIcon = computed((): IconName => {
    switch (role) {
        case "member":
            return "UserIcon";
        case "manager":
            return "CheckBadgeIcon";
        case "admin":
            return "ShieldCheckIcon";
        default:
            return "ExclamationCircleIcon";
    }
});
</script>

<template>
    <HeroIcon :icon="roleIcon" size="lg" type="solid" class="text-accent" />
</template>
