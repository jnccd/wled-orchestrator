import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getWledOrchState,
  setGroupTheme,
  wledOrchStateQueryKey,
} from "../../hooks/useWledOrchApi";
import { useSelectedGroupStore } from "../../hooks/useLocalStore";
import { Colorful, ColorResult } from "@uiw/react-color";
import { useState } from "react";
import { readProperty, writeProperty } from "../../utils/untypedPropertyAccess";

interface Props {
  propertyName: string;
}

const ThemePropertyColorEditor = ({ propertyName }: Props) => {
  const [refreshBool, refresh] = useState(false);

  // React Query setup
  const queryClient = useQueryClient();
  const wledOrchStateQuery = useQuery({
    queryKey: [wledOrchStateQueryKey],
    queryFn: getWledOrchState,
  });
  const changeThemeMutation = useMutation({
    mutationFn: setGroupTheme,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [wledOrchStateQueryKey] });
    },
  });

  const selectedGroupStore = useSelectedGroupStore();
  const selectedGroup = wledOrchStateQuery.data?.groups?.filter(
    (x) => x.id === selectedGroupStore.selectedGroup
  )[0];

  const propertyValue = readProperty(selectedGroup?.theme, propertyName);

  return (
    <Colorful
      color={{
        h: propertyValue.h,
        s: propertyValue.s,
        v: propertyValue.v,
        a: 1,
      }}
      onChange={(colorRes: ColorResult) => {
        writeProperty(selectedGroup?.theme, propertyName, colorRes.hsv);
        refresh(!refreshBool);
      }}
      onMouseUp={() => {
        changeThemeMutation.mutate({
          groupId: selectedGroup?.id ?? "",
          newTheme: selectedGroup?.theme,
        });
      }}
      disableAlpha
    ></Colorful>
  );
};

export default ThemePropertyColorEditor;
