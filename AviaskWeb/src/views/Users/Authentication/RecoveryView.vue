<script lang="ts" setup>
import useEventBus from "@/eventBus";
import { requestModels } from "@/models";
import { ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { ButtonComponent, TextInput, TitleComponent } from "@/components";
import useAuthenticationRepository from "@/repositories/authenticationRepository";

const route = useRoute();
const router = useRouter();
const { emitBus } = useEventBus();
const authenticationRepository = useAuthenticationRepository();

const model = ref<requestModels.PasswordResetModel>({
    token: route.params.token,
} as requestModels.PasswordResetModel);

async function onSubmit() {
    const result = await authenticationRepository.passwordReset(model.value);

    if (result) {
        emitBus("flashOpen", result, "danger");
        return;
    }

    emitBus("flashOpen", "Successfully reset your password", "success");
    router.push("/users/login");
}
</script>
<template>
    <div class="flex flex-col items-center py-6">
        <TitleComponent class="pb-8">Recover your account</TitleComponent>

        <form class="w-3/4 md:w-3/5 lg:w-2/5 flex flex-col justify-center gap-2" novalidate @submit.prevent="onSubmit">
            <div class="flex flex-col gap-1">
                <label class="text-xl dark:text-white font-semibold">
                    New password
                    <span class="text-red-500">*</span>
                </label>
                <TextInput v-model.trim="model.password" type="password" required minlength="6" maxlength="32" />
            </div>

            <div class="flex flex-col gap-1">
                <label class="text-xl dark:text-white font-semibold">
                    Confirm the new password
                    <span class="text-red-500">*</span>
                </label>
                <TextInput v-model.trim="model.rePassword" type="password" required minlength="6" maxlength="32" />
            </div>

            <div class="flex justify-end gap-2 mt-4">
                <ButtonComponent type="submit" state="accent">Recover my account</ButtonComponent>
                <ButtonComponent button-type="secondary" router-link="/users/login" state="cta">Login</ButtonComponent>
            </div>
        </form>
    </div>
</template>
