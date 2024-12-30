import { useQuery } from "@tanstack/react-query";
import { create } from "zustand/react";
import { getWledOrchState, wledOrchStateQueryKey } from "./useWledOrchState";

export interface SelectedGroupStore {
    selectedGroup: string;
    initialize: () => void;
    selectNew: (newSelectedGroup: string) => void;
}

const useSelectedGroupStore = create<SelectedGroupStore>(set => ({
    selectedGroup: "",
    initialize: () => set(set => {
        const groups = useQuery({
            queryKey: [wledOrchStateQueryKey],
            queryFn: getWledOrchState,
          }).data?.groups

        if (groups?.filter(x => x.id === set.selectedGroup).length === 0) {
            return { selectedGroup: groups?.at(0)?.id }
        }

        return set;
    }),
    selectNew: (newSelectedGroup) => set(_ => ({ selectedGroup: newSelectedGroup}) )
}));

export default useSelectedGroupStore;