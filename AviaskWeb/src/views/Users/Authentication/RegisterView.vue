<script lang="ts" setup>
import useEventBus from "@/eventBus";
import { requestModels } from "@/models";
import useUserStore from "@/stores/userStore";
import Cookies from "js-cookie";
import { onMounted, ref, type Ref } from "vue";
import { useRouter } from "vue-router";
import { ButtonComponent, TextInput, TitleComponent } from "@/components";
import type { CreateUserModel } from "@/models/DTO/requestModels";
import useUsersRepository from "@/repositories/usersRepository";

const usersRepository = useUsersRepository();
const userStore = useUserStore();
const router = useRouter();
const { emitBus } = useEventBus();

const createUserModel: Ref<requestModels.CreateUserModel> = ref<requestModels.CreateUserModel>({} as CreateUserModel);
const errorMessages: Ref<string[]> = ref<string[]>([]);

async function onSubmit() {
    const res = await usersRepository.create(createUserModel.value);

    if (!res.success) {
        emitBus("flashOpen", res.error.message, "danger");
        errorMessages.value = [res.error.message];

        return;
    }

    Cookies.set("token", res.result.token);
    userStore.authenticate();

    emitBus("flashOpen", "Welcome to Aviask !");
    router.push("/user/dashboard");
}

onMounted(() => {
    if (userStore.isAuthenticated) {
        router.push("/");
    }
});
</script>

<template>
    <div class="flex flex-col items-center py-6">
        <TitleComponent class="mb-5 md:mb-8 whitespace-nowrap text-center">Create an account</TitleComponent>

        <form class="w-full sm:w-4/5 md:w-3/5 lg:w-1/2 flex flex-col justify-center gap-4" @submit.prevent="onSubmit">
            <p
                v-if="errorMessages?.length"
                class="flex flex-col gap-2 mt-2 mb-6 bg-red-200 font-semibold text-red-800 rounded-lg mx-2 px-6 py-5">
                <span v-for="message in errorMessages" :key="message">
                    {{ message }}
                </span>
            </p>

            <div class="flex flex-col gap-1">
                <label class="text-xl dark:text-white font-medium md:font-semibold">
                    Username
                    <sup class="text-red-500">*</sup>
                </label>
                <TextInput v-model.trim="createUserModel.username" type="text" required minlength="3" maxlength="24" />
            </div>

            <div class="flex flex-col gap-1">
                <label class="text-xl dark:text-white font-medium md:font-semibold">
                    Email
                    <sup class="text-red-500">*</sup>
                </label>
                <TextInput v-model.trim="createUserModel.email" type="email" required />
            </div>

            <div class="flex flex-col gap-1">
                <label class="text-xl dark:text-white font-medium md:font-semibold">
                    Password
                    <sup class="text-red-500">*</sup>
                </label>
                <TextInput
                    v-model.trim="createUserModel.password"
                    type="password"
                    required
                    minlength="6"
                    maxlength="32" />
            </div>

            <p class="py-2 font-medium text-gray-700 dark:text-gray-200">
                By clicking on Register, you agree our
                <RouterLink to="/tos" class="underline underline-offset-2">Terms of Service</RouterLink>
            </p>

            <div class="grid grid-cols-2 gap-2 mt-2">
                <ButtonComponent type="submit" state="accent">Register</ButtonComponent>
                <ButtonComponent button-type="secondary" router-link="/users/login" state="cta">Login</ButtonComponent>
            </div>
        </form>
    </div>
</template>
