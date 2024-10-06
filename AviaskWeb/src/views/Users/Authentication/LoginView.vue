<script lang="ts" setup>
import useEventBus from "@/eventBus";
import useUserStore from "@/stores/userStore";
import Cookies from "js-cookie";
import { onMounted, ref, type Ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { requestModels } from "@/models";
import { ButtonComponent, HeroIcon, TextInput, TitleComponent } from "@/components";
import { z } from "zod";
import useAuthenticationRepository from "@/repositories/authenticationRepository";

type ViewMode = "login" | "passwordReset";

const redirectQuerySchema = z.object({
    redirectTo: z.string().nullable().catch(null),
});

const authenticationRepository = useAuthenticationRepository();
const userStore = useUserStore();
const router = useRouter();
const route = useRoute();
const { emitBus } = useEventBus();

const redirectQueryValue = redirectQuerySchema.parse({
    redirectTo: route.query.redirectTo,
});

const loginModel: Ref<requestModels.LoginModel> = ref<requestModels.LoginModel>({
    email: "",
    password: "",
});
const mode = ref<ViewMode>("login");

async function onLogin() {
    const res = await authenticationRepository.login(loginModel.value);

    if (res.success && res.result.token) {
        Cookies.set("token", res.result.token);
        userStore.authenticate();

        emitBus("flashOpen", "You are successfully authenticated", "success");

        return router.push(redirectQueryValue.redirectTo ?? "/user/dashboard");
    }

    emitBus("flashOpen", "Invalid credentials", "danger");
}

async function onResetPassword() {
    const success = await authenticationRepository.recovery(loginModel.value.email);

    if (!success) {
        emitBus("flashOpen", "The server could not send you a reset link", "danger");
        return;
    }

    emitBus("flashOpen", "An email containing a link to reset your password has been sent", "success");
    mode.value = "login";
}

onMounted(() => {
    if (userStore.isAuthenticated) {
        router.push("/");
    }
});
</script>

<template>
    <template v-if="mode === 'login'">
        <div class="flex flex-col items-center py-6">
            <TitleComponent class="pb-8">Log in</TitleComponent>

            <form
                class="w-full sm:w-4/5 md:w-3/5 lg:w-1/2 flex flex-col justify-center gap-4"
                @submit.prevent="onLogin">
                <div class="flex flex-col gap-1">
                    <label class="text-xl dark:text-white font-medium md:font-semibold">
                        Email
                        <sup class="text-red-500">*</sup>
                    </label>
                    <TextInput v-model.trim="loginModel.email" type="email" required />
                </div>

                <div class="flex flex-col gap-1">
                    <label class="text-xl dark:text-white font-medium md:font-semibold">
                        Password
                        <sup class="text-red-500">*</sup>
                    </label>
                    <TextInput v-model.trim="loginModel.password" type="password" required />
                </div>

                <div class="grid sm:grid-cols-2 gap-2">
                    <span
                        class="cursor-pointer font-medium text-sm sm:text-base hover:underline underline-offset-2 flex gap-1 items-center dark:text-gray-200 text-gray-700"
                        @click="mode = 'passwordReset'">
                        <HeroIcon icon="QuestionMarkCircleIcon" />
                        <span>Forgot your password ?</span>
                    </span>

                    <div class="grid grid-cols-2 w-full gap-2 mt-4 ml-auto">
                        <ButtonComponent
                            type="button"
                            button-type="secondary"
                            router-link="/users/register"
                            state="accent">
                            Register
                        </ButtonComponent>
                        <ButtonComponent type="submit" state="cta">Login</ButtonComponent>
                    </div>
                </div>
            </form>
        </div>
    </template>
    <template v-else>
        <div class="flex flex-col items-center py-6">
            <TitleComponent class="pb-8 text-balance text-center">Reset your password</TitleComponent>

            <form
                class="w-full sm:w-4/5 md:w-3/5 lg:w-1/2 flex flex-col justify-center gap-4"
                @submit.prevent="onResetPassword">
                <div class="flex flex-col gap-1">
                    <label class="text-xl dark:text-white font-semibold">
                        Email
                        <sup class="text-red-500">*</sup>
                    </label>
                    <TextInput v-model.trim="loginModel.email" type="email" required />
                </div>

                <div class="flex justify-between gap-2 mt-4">
                    <span
                        class="font-medium text-sm sm:text-base hover:underline underline-offset-2 flex gap-1 items-center dark:text-gray-200 text-gray-700"
                        @click="mode = 'login'">
                        <HeroIcon icon="ChevronLeftIcon" />
                        <span>Back to login</span>
                    </span>
                    <ButtonComponent type="submit" state="cta" class="flex gap-2 items-center">
                        <HeroIcon icon="PaperAirplaneIcon" type="solid" />
                        <span>Submit</span>
                    </ButtonComponent>
                </div>
            </form>
        </div>
    </template>
</template>
