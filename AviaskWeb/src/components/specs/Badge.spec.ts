import { mount } from "@vue/test-utils";
import BadgeComponent from "../Widgets/BadgeComponent.vue";
import { it, expect } from "vitest";

const wrapper = mount(BadgeComponent, {
    props: {
        badgeType: "red",
    },
    slots: {
        default: "Badge Text",
    },
});

it("testing BadgeComponent component props", async () => {
    expect(BadgeComponent.props.badgeType.required).true;
});

it("testing BadgeComponent component slots", async () => {
    expect(wrapper.html()).toContain("Badge Text");
});
