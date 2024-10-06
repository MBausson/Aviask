import { defineStore } from "pinia";
import Cookies from "js-cookie";
import { UserRole, type UserRoleValue } from "@/models/aviaskUser";
import { type Ref, ref, computed } from "vue";
import { AviaskUser } from "@/models/aviaskUser";
import useEventBus from "@/eventBus";
import { jwtDecode } from "jwt-decode";
import useAuthenticationRepository from "@/repositories/authenticationRepository";

const useUserStore = defineStore("userStore", () => {
    const triedLogin = ref(false);
    const userDetails: Ref<AviaskUser | null> = ref(null);

    const TOKEN_LIFESPAN = 1000 * 60 * 30; // 30 minutes - complete lifespan of our jwt
    const TOKEN_MIN_DURATION = 1000 * 60 * 10; // 10 minutes - lifespan from which we need to refresh our jwt

    const userHasRole = (roleValue: UserRoleValue): boolean => {
        return (
            userDetails.value !== null &&
            UserRole.getUserRole(roleValue).hierarchyValue <= userDetails.value.role.hierarchyValue!
        );
    };

    const isAuthenticated = computed<boolean>(() => userDetails.value != null);

    async function authenticate() {
        triedLogin.value = true;
        userDetails.value = null;

        const cookieToken = Cookies.get("token");
        
        if (!cookieToken) return;

        const authenticationRepository = useAuthenticationRepository();
        const res = await authenticationRepository.authenticate(cookieToken);
        
        if (!res.success || userDetails.value) return;

        userDetails.value = new AviaskUser(res.result);
        await scheduleJwtRefresh();
    }

    function logout() {
        Cookies.remove("token");
        userDetails.value = null;
    }

    //  Plan the refresh of the user's JWT 10 minutes before it expires
    async function scheduleJwtRefresh() {
        const currentToken = Cookies.get("token") || "";

        const currentTokenLifespan = getTokenLifespanInMs(currentToken);

        setTimeout(async () => {
            if (!isAuthenticated.value) return;

            await refreshJwt();

            await scheduleJwtRefresh();
        }, currentTokenLifespan - TOKEN_MIN_DURATION);
    }

    //  Asks the API to refresh the current token
    async function refreshJwt() {
        const authenticationRepository = useAuthenticationRepository();
        const res = await authenticationRepository.refresh();

        if (!res.success) {
            const { emitBus } = useEventBus();

            console.error(res.error.message);
            emitBus("flashOpen", res.error.message, "danger");

            logout();
            return;
        }

        Cookies.set("token", res.result.token, {
            sameSite: "Strict",
            expires: Date.now() + TOKEN_LIFESPAN,
        });
    }

    //  Returns the lifespan of a JWT
    //  If negative, the JWT is eiher expired or could not be decoded
    function getTokenLifespanInMs(token: string): number {
        let exp = -1;

        try {
            exp = jwtDecode(token).exp!;
        } catch (e) {
            exp = -1;
        }

        return exp * 1000 - new Date().getTime();
    }

    return {
        triedLogin,
        userDetails,
        userHasRole,
        authenticate,
        isAuthenticated,
        logout,
    };
});

export default useUserStore;
