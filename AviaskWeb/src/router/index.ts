import useEventBus from "@/eventBus";
import type { UserRoleValue } from "@/models/aviaskUser";
import useUserStore from "@/stores/userStore";
import { createRouter, createWebHistory } from "vue-router";

const router = createRouter({
    history: createWebHistory(),
    routes: [
        {
            path: "/:catchAll(.*)",
            alias: "/:catchAll(.*)",
            name: "Not found",
            component: () => import("@/views/NotFoundView.vue"),
        },
        {
            path: "/",
            name: "The modern ATPL bank",
            component: () => import("@/views/HomeView.vue"),
            meta: {
                redirectIfAuthenticated: "/user/dashboard",
            },
        },
        {
            path: "/aboutus",
            name: "About us",
            component: () => import("@/views/Legal/AboutUsView.vue"),
        },
        {
            path: "/privacy",
            name: "Privacy policy",
            component: () => import("@/views/Legal/PrivacyPolicyView.vue"),
        },
        {
            path: "/tos",
            name: "Terms of Service",
            component: () => import("@/views/Legal/TermsOfServiceView.vue"),
        },
        {
            path: "/payments/pricing",
            name: "Pricing",
            component: () => import("@/views/Payments/PricingView.vue"),
        },
        {
            path: "/payments/result",
            name: "Your checkout",
            component: () => import("@/views/Payments/CheckoutResultView.vue"),
            meta: {
                requiredRole: "member",
            },
        },
        {
            path: "/payments/current",
            name: "Your current subscription",
            component: () => import("@/views/Payments/CurrentSubscriptionView.vue"),
            meta: {
                requiredRole: "member",
            },
        },
        {
            path: "/users",
            name: "Manage users",
            component: () => import("@/views/Users/UsersView.vue"),
            meta: {
                requiredRole: "admin",
            },
        },
        {
            path: "/users/register",
            name: "Register",
            component: () => import("@/views/Users/Authentication/RegisterView.vue"),
        },
        {
            path: "/users/login",
            name: "Login",
            component: () => import("@/views/Users/Authentication/LoginView.vue"),
        },
        {
            path: "/user/:id?/profile",
            name: "Profile",
            component: () => import("@/views/Users/UserView.vue"),
        },
        {
            path: "/user/dashboard",
            name: "Dashboard",
            component: () => import("@/views/Users/DashboardView.vue"),
            meta: {
                requiredRole: "member",
            },
        },
        {
            path: "/user/:id/answerRecords",
            name: "Answer records",
            component: () => import("@/views/Users/AnswerRecordsView.vue"),
        },
        {
            path: "/user/:id/edit",
            name: "Edit user",
            component: () => import("@/views/Users/EditUserView.vue"),
            meta: {
                requiredRole: "admin",
            },
        },
        {
            path: "/users/recovery/:token",
            name: "Account recovery",
            component: () => import("@/views/Users/Authentication/RecoveryView.vue"),
        },
        {
            path: "/user/publications",
            name: "Your publications",
            component: () => import("@/views/Users/PublicationsView.vue"),
        },
        {
            path: "/questions",
            name: "Questions bank",
            component: () => import("@/views/Questions/QuestionsView.vue"),
        },
        {
            path: "/question/:id",
            name: "Question",
            component: () => import("@/views/Questions/QuestionView.vue"),
        },
        {
            path: "/question/:id/edit",
            name: "Edit question",
            component: () => import("@/views/Questions/EditQuestionView.vue"),
            meta: {
                requiredRole: "manager",
            },
        },
        {
            path: "/questions/new",
            name: "New question",
            component: () => import("@/views/Questions/NewQuestionView.vue"),
            meta: {
                requiredRole: "manager",
            },
        },
        {
            path: "/questions/reports",
            name: "Question reports",
            component: () => import("@/views/Questions/QuestionReportsView.vue"),
            meta: {
                requiredRole: "manager",
            },
        },
        {
            path: "/suggestions/new",
            name: "Suggest a new question",
            component: () => import("@/views/Suggestions/NewSuggestionView.vue"),
            meta: {
                requiredRole: "member",
            },
        },
        {
            path: "/suggestion/:id",
            name: "Edit a suggestion",
            component: () => import("@/views/Suggestions/EditSuggestionView.vue"),
            meta: {
                requiredRole: "manager",
            },
        },
        {
            path: "/suggestions",
            name: "List of suggestions",
            component: () => import("@/views/Suggestions/SuggestionsView.vue"),
            meta: {
                requiredRole: "manager",
            },
        },
        {
            path: "/mockExam",
            name: "Practice exams",
            component: () => import("@/views/MockExams/MockExamView.vue"),
            meta: {
                requiredRole: "user",
            },
        },
        {
            path: "/mockExams",
            name: "Practice exams history",
            component: () => import("@/views/MockExams/MockExamsView.vue"),
            meta: {
                requiredRole: "user",
            },
        },
        {
            path: "/mockExam/question/:id",
            name: "Practice exam question",
            component: () => import("@/views/MockExams/MockExamQuestionView.vue"),
            meta: {
                requiredRole: "user",
            },
        },
        {
            path: "/leaderboard",
            name: "Leaderboard",
            component: () => import("@/views/Leaderboard/LeaderboardView.vue"),
            meta: {},
        },
    ],
    scrollBehavior(to, from) {
        if (to.path !== from.path) return { top: 0, left: 0 };
    },
});

router.beforeEach(async (to, from, next) => {
    const userStore = useUserStore();
    const { emitBus } = useEventBus();

    //  If the user has not tried to authenticate himself, do it
    if (!userStore.isAuthenticated && userStore.triedLogin) await userStore.authenticate();

    if (to.meta.requiredRole && !userStore.userHasRole(to.meta.requiredRole as UserRoleValue)) {
        emitBus("flashOpen", "You are not authorized to access this page", "danger");

        return next(userStore.isAuthenticated ? "/" : `/users/login?redirectTo=${to.path}`);
    }

    if (to.meta.redirectIfAuthenticated && userStore.isAuthenticated) {
        return next(to.meta.redirectIfAuthenticated);
    }

    if (to.name) {
        document.title = `${to.name?.toString()} - Aviask`;
    } else {
        document.title = "Aviask";
    }

    next();
});

export default router;
