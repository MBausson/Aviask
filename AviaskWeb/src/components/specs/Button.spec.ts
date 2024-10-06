import { mount } from "@vue/test-utils";
import ButtonComponent from "../Widgets/ButtonComponent.vue";
import { it, expect } from "vitest";

const wrapper = mount(ButtonComponent, {
    slots: {
        default: "Dummy button",
        routerLink: "google.com",
    },
});

it("testing Button component props", async () => {
    //  state
    expect(ButtonComponent.props.state).toBeTruthy();
    expect(ButtonComponent.props.state.type()).toBeTypeOf("string");
    expect(ButtonComponent.props.state.required).false;
    expect(ButtonComponent.props.state.default()).toEqual("raised");

    //  routerLink
    expect(ButtonComponent.props.routerLink).toBeTruthy();
    expect(ButtonComponent.props.routerLink.type()).toBeTypeOf("string");
    expect(ButtonComponent.props.routerLink.required).false;
    expect(ButtonComponent.props.routerLink.default).undefined;
});

it("testing ButtonComponent component slots", async () => {
    expect(wrapper.html()).toContain("Dummy button");
});

it("testing ButtonComponent component element", async () => {
    expect(wrapper.element).toBe<HTMLButtonElement>;
});
