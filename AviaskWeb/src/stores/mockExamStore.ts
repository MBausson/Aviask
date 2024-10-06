import useEventBus from "@/eventBus";
import { type AviaskMockExam } from "@/models/aviaskMockExam";
import useMockExamsRepository from "@/repositories/mockExamsRepository";
import moment from "moment";
import { defineStore } from "pinia";
import { computed, ref } from "vue";
import { useRouter } from "vue-router";

const useMockExamStore = defineStore("mockExamStore", () => {
    const router = useRouter();
    const { emitBus } = useEventBus();
    const mockExamRepository = useMockExamsRepository();
    const current = ref<AviaskMockExam | null>();

    const hasNextQuestion = computed<boolean>(() => {
        if (!current.value) return false;

        return current.value.maxQuestions > current.value.answerRecords.length;
    });

    async function updateCurrent() {
        current.value = await mockExamRepository.current();

        if (!current.value) return;

        const remainingDuration = moment.duration(
            moment(current.value.startedAt)
                .add(current.value.maxDuration, "milliseconds")
                .diff(moment(), "milliseconds"),
        );

        //  Keeping in mind that the inital sate may be != to the state the timeout triggers
        setTimeout(
            () => {
                if (!current.value) return;

                router.push("/mockExam");
                emitBus("flashOpen", "You ran out of time for your practice exam !", "success");

                current.value = null;
            },
            remainingDuration.as("milliseconds") + 500,
        );
    }

    return { current, hasNextQuestion, updateCurrent };
});

export default useMockExamStore;
