import { defineStore } from "pinia";
import useUserStore from "./userStore";
import Cookies from "js-cookie";
import axios, { AxiosError } from "axios";
import useEventBus from "@/eventBus";
import { useRouter } from "vue-router";
import type { ApiResponse } from "@/models/DTO/responseModels";

export type RequestFunction = <T>(method: RequestMethod, url: string, data?: unknown) => Promise<ApiResponse<T>>;

export type FormRequestFunction = <T>(
    method: RequestMethod,
    url: string,
    fields: Record<string, string | Blob | null>,
) => Promise<T>;

type RequestMethod = "get" | "post" | "patch" | "put" | "delete";

const useApiStore = defineStore("apiStore", () => {
    const userStore = useUserStore();
    const { emitBus } = useEventBus();
    const router = useRouter();

    const baseUrl = import.meta.env.VITE_APP_API_BASE_URL;

    const axiosInstance = axios.create({
        baseURL: baseUrl,
    });

    const getFullUrl = (endpoint: string): string => `${baseUrl}${endpoint}`;

    axiosInstance.interceptors.request.use(async config => {
        const cookieToken = Cookies.get("token");

        if (cookieToken) {
            config.headers.Authorization = `Bearer ${cookieToken}`;
        }

        return config;
    });

    axiosInstance.interceptors.response.use(undefined, (error: AxiosError) => {
        console.warn(`[${error.status}] => ${error.request.url} - ${error.response?.data}`);

        switch (error.response?.status) {
            case 401:
            case 403:
                emitBus("flashOpen", "Your are not authorized to perform this action", "danger");

                router.push(userStore.isAuthenticated ? "/" : `/users/login`);
                break;

            case 429:
                emitBus("flashOpen", "You are sending too many requests to the server", "danger");
                break;

            case 500:
                emitBus("flashOpen", "The server could not process your request", "danger");
                break;
        }
    });

    const $requestForm: FormRequestFunction = async function <T>(
        method: RequestMethod,
        url: string,
        fields: Record<string, string | Blob | null>,
    ): Promise<T> {
        const form = new FormData();

        for (const key in fields) {
            form.append(key, fields[key] || "");
        }

        return axiosInstance[method]<T>(url, form)
            .then(r => {
                return r.data;
            })
            .catch(e => {
                throw e;
            });
    };

    const $request: RequestFunction = <T>(
        method: RequestMethod,
        url: string,
        data: unknown = null,
    ): Promise<ApiResponse<T>> => {
        return axiosInstance[method]<T>(url, data, {
            headers: {
                "Content-Type": "application/json",
            },
            validateStatus: () => true,
        })
            .then(r => {
                const isSuccess = r.status >= 200 && r.status < 300;

                return {
                    result: isSuccess && r.data,
                    error: !isSuccess && r.data,
                    success: isSuccess,
                } as ApiResponse<T>;
            })
            .catch(e => {
                throw e;
            });
    };

    return {
        $request,
        $requestForm,
        getFullUrl,
    };
});

export default useApiStore;
