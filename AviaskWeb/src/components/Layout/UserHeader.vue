<script lang="ts" setup>
import useEventBus from "@/eventBus";
import useUserStore from "@/stores/userStore";
import { useRouter } from "vue-router";
import { ButtonComponent, HeroIcon } from "@/components";
import useThemeStore from "@/stores/themeStore";
import {
    DropdownMenu,
    DropdownMenuTrigger,
    DropdownMenuItem,
    DropdownMenuSeparator,
    DropdownMenuContent,
} from "@/components/ui/dropdown-menu";

const userStore = useUserStore();
const themeStore = useThemeStore();
const router = useRouter();
const { emitBus } = useEventBus();

await userStore.authenticate();

function onLogout() {
    userStore.logout();
    router.push("/");

    emitBus("flashOpen", "You are now logged out", "success");
}
</script>

<template>
    <div class="flex gap-1 sm:gap-3">
        <template v-if="!(userStore.isAuthenticated && userStore.userDetails)">
            <div class="flex items-center gap-2.5 sm:gap-3.5 md:gap-5">
                <RouterLink to="/users/login">
                    <button class="font-medium md:font-semibold sm:text-lg">Login</button>
                </RouterLink>

                <ButtonComponent router-link="/users/register" :state="themeStore.isDark ? 'accent' : 'cta'">
                    Sign up
                </ButtonComponent>
            </div>
        </template>

        <template v-else>
            <DropdownMenu>
                <DropdownMenuTrigger>
                    <div
                        class="text-white bg-black rounded-lg px-3 py-2 group transition-all h-12 sm:h-14 flex items-center gap-2">
                        <HeroIcon icon="UserCircleIcon" type="solid" />
                        <span class="font-medium tracking-wide hidden sm:inline first-letter:capitalize">
                            {{ userStore.userDetails.username.slice(0, 12) }}
                        </span>
                    </div>
                </DropdownMenuTrigger>

                <DropdownMenuContent class="min-w-56">
                    <DropdownMenuItem>
                        <RouterLink
                            :to="`/user/${userStore.userDetails.id}/profile`"
                            class="flex items-center gap-3 font-semibold">
                            <HeroIcon icon="FaceSmileIcon" />
                            <span class="first-letter:capitalize">{{ userStore.userDetails.username }}</span>
                        </RouterLink>
                    </DropdownMenuItem>

                    <DropdownMenuSeparator />

                    <DropdownMenuItem>
                        <RouterLink :to="`/user/dashboard`" class="flex items-center gap-3 font-medium">
                            <HeroIcon icon="RectangleGroupIcon" />
                            <span>Dashboard</span>
                        </RouterLink>
                    </DropdownMenuItem>

                    <DropdownMenuItem>
                        <RouterLink :to="`/mockExams`" class="flex items-center gap-3 font-medium">
                            <HeroIcon icon="TrophyIcon" />
                            <span>Practice exams</span>
                        </RouterLink>
                    </DropdownMenuItem>

                    <DropdownMenuItem>
                        <RouterLink
                            :to="`/user/${userStore.userDetails.id}/answerRecords`"
                            class="flex items-center gap-3 h-full font-medium">
                            <HeroIcon icon="ListBulletIcon" />
                            <span>Answer records</span>
                        </RouterLink>
                    </DropdownMenuItem>

                    <DropdownMenuItem>
                        <RouterLink :to="`/user/publications`" class="flex items-center gap-3 h-full font-medium">
                            <HeroIcon icon="BookOpenIcon" />
                            <span>Your publications</span>
                        </RouterLink>
                    </DropdownMenuItem>

                    <DropdownMenuItem v-if="userStore.userDetails.isPremium">
                        <RouterLink :to="`/payments/current`" class="flex items-center gap-3 h-full font-medium">
                            <HeroIcon icon="BanknotesIcon" />
                            <span>Premium subscription</span>
                        </RouterLink>
                    </DropdownMenuItem>

                    <DropdownMenuItem v-if="userStore.userHasRole('admin')">
                        <RouterLink :to="`/users`" class="flex items-center gap-3 h-full font-medium">
                            <HeroIcon icon="UsersIcon" />
                            <span>Manage users</span>
                        </RouterLink>
                    </DropdownMenuItem>

                    <DropdownMenuItem v-if="userStore.userHasRole('admin')">
                        <RouterLink :to="`/questions/reports`" class="flex items-center gap-3 h-full font-medium">
                            <HeroIcon icon="FlagIcon" />
                            <span>Question reports</span>
                        </RouterLink>
                    </DropdownMenuItem>

                    <DropdownMenuItem v-if="userStore.userHasRole('admin')">
                        <RouterLink :to="`/suggestions`" class="flex items-center gap-3 h-full font-medium">
                            <HeroIcon icon="BeakerIcon" type="solid" />
                            <span>Question suggestions</span>
                        </RouterLink>
                    </DropdownMenuItem>

                    <DropdownMenuSeparator />

                    <DropdownMenuItem @click="onLogout">
                        <button class="flex items-center gap-3 h-full text-red-400 font-semibold tracking-wide">
                            <HeroIcon icon="ArrowLeftStartOnRectangleIcon" />
                            <span>Log out</span>
                        </button>
                    </DropdownMenuItem>
                </DropdownMenuContent>
            </DropdownMenu>
        </template>
    </div>
</template>
