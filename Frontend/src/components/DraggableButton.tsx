import { Button } from "@chakra-ui/react";
import React, { useState } from "react";

const debuggingLogs = false;

interface Props {
  ownId: string;
  buttonName: string;
  onDragEnd?: (elem: HTMLElement, mousePos: number[]) => void;
}

const DraggableButton = ({ ownId, buttonName, onDragEnd }: Props) => {
  const [dragging, setDragging] = useState(false);
  const [draggingStartPos, setDraggingStartPos] = useState([0, 0]);
  const [draggingLastPos, setDraggingLastPos] = useState([0, 0]);
  const dragY = false;
  const dragAreaPaddingX = 200;

  const dragMouseDown = (e: React.MouseEvent) => {
    if (debuggingLogs) console.log("dragMouseDown");
    e.preventDefault();

    var thisInDocument = document.getElementById(ownId);
    if (thisInDocument === null) {
      if (debuggingLogs) console.log("dragMouseMove, cant find thisInDocument");
      return;
    }

    setDraggingStartPos([
      e.clientX - (parseInt(thisInDocument.style.left.split("p")[0]) || 0),
      e.clientY - (parseInt(thisInDocument.style.top.split("p")[0]) || 0),
    ]);
    thisInDocument.style.transition = "";
    thisInDocument.style.zIndex = "10";
    if (debuggingLogs) console.log(draggingStartPos);
    setDragging(true);
  };

  const dragMouseMove = (e: MouseEvent) => {
    if (debuggingLogs) console.log("dragMouseMove, " + dragging + ownId);
    if (!dragging) {
      return;
    }

    e.preventDefault();

    var thisInDocument = document.getElementById(ownId);
    if (thisInDocument === null) {
      if (debuggingLogs) console.log("dragMouseMove, cant find thisInDocument");
      return;
    }

    setPos(
      thisInDocument,
      e.clientX - draggingStartPos[0],
      e.clientY - draggingStartPos[1]
    );

    setDraggingLastPos([e.clientX, e.clientY]);
  };

  const dragMouseUp = () => {
    if (debuggingLogs) console.log("dragMouseUp");
    var thisInDocument = document.getElementById(ownId);
    if (thisInDocument === null) {
      if (debuggingLogs) console.log("dragMouseMove, cant find thisInDocument");
      return;
    }

    thisInDocument.style.transition = "left 0.4s";
    thisInDocument.style.zIndex = "1";
    setPos(thisInDocument, 0, 0);
    setDragging(false);
    if (onDragEnd) onDragEnd(thisInDocument, draggingLastPos);
  };

  const setPos = (e: HTMLElement, x: number, y: number) => {
    e.style.left = x + "px";
    if (dragY) {
      e.style.top = y + "px";
    }

    const boundingRect = e.getBoundingClientRect();
    if (boundingRect.left < dragAreaPaddingX) {
      if (debuggingLogs) console.log("too far left!");
      const left = parseInt(e.style.left.split("p")[0]);
      e.style.left = String(left + (dragAreaPaddingX - boundingRect.x)) + "px";
    }
    if (boundingRect.right > document.body.clientWidth - dragAreaPaddingX) {
      if (debuggingLogs) console.log("too far right!");
      const left = parseInt(e.style.left.split("p")[0]);
      e.style.left =
        String(
          left +
            (document.body.clientWidth - dragAreaPaddingX - boundingRect.right)
        ) + "px";
    }
  };

  document.onmousemove = dragMouseMove;
  document.onmouseup = dragMouseUp;

  return (
    <Button
      id={ownId}
      position={"relative"}
      className="wledServerButton"
      margin={2}
      size="lg"
      cursor={"move"}
      onMouseDown={dragMouseDown}
    >
      {buttonName}
    </Button>
  );
};

export default DraggableButton;
