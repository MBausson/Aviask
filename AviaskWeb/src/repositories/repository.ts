import type { FormRequestFunction, RequestFunction } from "@/stores/apiStore";

export abstract class Repository {
    protected $request: RequestFunction;
    protected $formRequest: FormRequestFunction;

    constructor(request: RequestFunction, formRequest: FormRequestFunction) {
        this.$request = request;
        this.$formRequest = formRequest;
    }
}
