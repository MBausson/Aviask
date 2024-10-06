// @/components/TextInput.spec.js
import { mount } from "@vue/test-utils";
import { describe, it, expect } from "vitest";
import { TextInput } from "@/components";

describe("TextInput component", () => {
    it("renders the input element", () => {
        const wrapper = mount(TextInput);
        expect(wrapper.find("input").exists()).toBe(true);
    });

    it("binds the input value correctly", async () => {
        const wrapper = mount(TextInput, {
            props: {
                modelValue: "initial",
                "update:modelValue": (e: string) => wrapper.setProps({ modelValue: e }),
            },
        });

        await wrapper.find("input").setValue("Test Input");

        // Check if the input value is correctly bound
        expect(wrapper.find("input").element.value).toBe("Test Input");
    });

    it("emits input event on value change", async () => {
        const wrapper = mount(TextInput);
        const input = wrapper.find("input");

        await input.setValue("Hello, Vitest!");

        expect(wrapper.emitted("update:modelValue")![0]).toEqual(["Hello, Vitest!"]);
    });
});
