import useApiStore from "@/stores/apiStore";
import { Repository } from "./repository";
import type { ApiResponse, AuthenticationResult, AviaskUserDetails } from "@/models/DTO/responseModels";
import type { requestModels, responseModels } from "@/models";

export class AuthenticationRepository extends Repository {
    private routeMap = {
        authenticate: () => `/api/authentication/authenticate`,
        login: () => `/api/authentication/login`,
        refresh: () => `/api/authentication/refresh`,
        recovery: () => `/api/authentication/recovery`,
        passwordReset: () => `/api/authentication/passwordReset`,
    };

    async authenticate(token: string): Promise<ApiResponse<AviaskUserDetails>> {
        return await this.$request<AviaskUserDetails>("post", this.routeMap.authenticate(), token);
    }

    async login(model: requestModels.LoginModel): Promise<ApiResponse<AuthenticationResult>> {
        const res = await this.$request<responseModels.AuthenticationResult>("post", this.routeMap.login(), model);

        return res;
    }

    async refresh(): Promise<ApiResponse<AuthenticationResult>> {
        return await this.$request<responseModels.AuthenticationResult>("get", this.routeMap.refresh());
    }

    async recovery(email: string): Promise<boolean> {
        const res = await this.$request("post", this.routeMap.recovery(), email);

        return res.success;
    }

    async passwordReset(passwordResetModel: requestModels.PasswordResetModel): Promise<string | null> {
        const res = await this.$request<string>("post", this.routeMap.passwordReset(), passwordResetModel);

        return res.success ? null : "An error occured while recovering your account";
    }
}

export default function useAuthenticationRepository() {
    const apiStore = useApiStore();

    return new AuthenticationRepository(apiStore.$request, apiStore.$requestForm);
}
