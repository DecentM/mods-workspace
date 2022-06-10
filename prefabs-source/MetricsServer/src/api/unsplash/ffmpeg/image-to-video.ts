import ffmpeg from 'ffmpeg-static'
import Fluent from 'fluent-ffmpeg'
import fs from 'node:fs'

Fluent.setFfmpegPath(ffmpeg)

export const imageToVideo = (imagePath: string) => {
  const read = fs.createReadStream(imagePath)
  const write = fs.createWriteStream(`${imagePath}_output.mp4`)

  const command = Fluent()
    .addInput(read)
    .inputFormat('jpg')
    .outputFormat('mp4')
    .videoBitrate('1024k')
    .videoCodec('mpeg4')
    .fpsOutput(25)
    .output(write)

  command.run()

  return {
    write,

    read: () => {
      return fs.createReadStream(write.path)
    },

    cleanup: () => {
      return fs.promises.unlink(write.path)
    },
  }
}
