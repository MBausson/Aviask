import useApiStore from "@/stores/apiStore";
import { Repository } from "./repository";
import type { responseModels } from "@/models";
import { AxiosError } from "axios";
import type { ApiResponse } from "@/models/DTO/responseModels";

export class PaymentsRepository extends Repository {
    private routeMap = {
        checkout: () => `/api/payments`,
        current: () => `/api/payments/current`,
        cancel_current: () => `/api/payments/current/cancel`,
    };

    async checkout(): Promise<ApiResponse<string>> {
        return await this.$request<string>("post", this.routeMap.checkout());
    }

    async current(): Promise<ApiResponse<responseModels.SubscriptionInformations>> {
        return await this.$request<responseModels.SubscriptionInformations>("get", this.routeMap.current());
    }

    async cancelCurrent(): Promise<boolean> {
        try {
            await this.$request<string>("post", this.routeMap.cancel_current());
            return true;
        } catch (e) {
            if (e instanceof AxiosError) return false;

            throw e;
        }
    }
}

export default function usePaymentsRepository() {
    const apiStore = useApiStore();

    return new PaymentsRepository(apiStore.$request, apiStore.$requestForm);
}
