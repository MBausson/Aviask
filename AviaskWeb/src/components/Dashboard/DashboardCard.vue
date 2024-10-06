<script lang="ts" setup></script>

<template>
    <section
        class="flex flex-col gap-5 p-6 rounded-2xl dark:text-white transition-all duration-500 hover:z-1 border-2 border-black/5 dark:border-white/10 hover:shadow-md">
        <h3 class="font-medium text-xl flex items-center gap-2">
            <slot name="title" />
        </h3>

        <div class="flex flex-col gap-4 dark:text-neutral-200 text-neutral-800">
            <slot name="content" />
        </div>
    </section>
</template>
